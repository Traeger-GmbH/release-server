//--------------------------------------------------------------------------------------------------
// <copyright file="FsReleaseArtifactRepository.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    internal class FsReleaseArtifactRepository : IReleaseArtifactRepository
    {
        #region ---------- Private constants ----------

        // Internal directory for uploads in progress (inside the artifact root, so moving is atomic).
        private const string TemporaryDirectoryName = ".temp";

        #endregion

        #region ---------- Private fields ----------
        
        private readonly DirectoryInfo artifactRootDir, backupRootDir;
        private ILogger logger;
        
        #endregion
        
        #region ---------- Public constructors ----------

        public FsReleaseArtifactRepository(
            ILogger<FsReleaseArtifactRepository> logger,
            DirectoryInfo artifactDirectory,
            DirectoryInfo backupDirectory)
        {
            artifactRootDir = artifactDirectory;
            backupRootDir = backupDirectory;
            this.logger = logger;
        }

        #endregion

        #region ---------- Public methods (by IReleaseArtifactRepository) ----------

        public void StoreArtifact(ReleaseArtifact artifact)
        {
            EnsureValidForStorage(artifact);

            var artifactPath = GenerateArtifactPath(
                artifact.DeploymentInformation.Identifier,
                artifact.DeploymentInformation.Os,
                artifact.DeploymentInformation.Architecture,
                artifact.DeploymentInformation.Version.ToString());

            var tmpDir = new DirectoryInfo(GenerateTemporaryPath());

            try
            {
                //Create the temporary directory
                if (!tmpDir.Exists)
                    tmpDir.Create();

                //Extract the payload to the temporary directory

                using (var outputStream = new FileStream(Path.Combine(tmpDir.ToString(), artifact.DeploymentMetaInformation.ArtifactFileName), FileMode.CreateNew)) {
                    artifact.Content.CopyTo(outputStream);
                }
                artifact.DeploymentMetaInformation.ToJsonFile(Path.Combine(tmpDir.ToString(), "deployment.json"));
                artifact.DeploymentInformation.ReleaseNotes
                    .ToJsonFile(Path.Combine(tmpDir.ToString(), artifact.DeploymentMetaInformation.ReleaseNotesFileName));
                logger.LogDebug("The Artifact was successfully unpacked & stored to the temp directory");

                var artifactDirectory = new DirectoryInfo(artifactPath);

                //If the directory already exists, delete the old content in there
                if (artifactDirectory.Exists)
                {
                    logger.LogDebug("This path already exists! Old content will be deleted!");
                }
                else
                {
                    artifactDirectory.Create();
                }

                tmpDir.Move(artifactDirectory, true);

                logger.LogInformation("The Artifact was successfully stored");
            }
            catch (Exception e)
            {
                logger.LogCritical(e.Message);
                throw;
            }
            finally
            {
                //Cleanup the tmp directory (also if storing failed)
                CleanupTemporaryDirectory(tmpDir);
            }
        }

        public List<DeploymentInformation> GetInfosByProductName(string productName)
        {
            var productInformation =
                from productDir in EnumerateProductDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                select TryGetDeploymentInformation(productDir.Name, osDir.Name, hwArchDir.Name, versionDir.Name);

            // Incomplete or broken artifact directories are skipped, so they cannot break the whole product.
            return productInformation.Where(deployment => deployment != null).ToList();
        }

        public DeploymentInformation GetDeploymentInformation(string productName, string os, string architecture, string version)
        {
            try
            {
                var path = GenerateArtifactPath(productName, os, architecture, version);

                if (path != null && Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();

                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);

                    var releaseNotesFileName = ResolveFileInDirectory(path, deploymentMetaInfo.ReleaseNotesFileName);

                    return new DeploymentInformation
                    {
                        ReleaseNotes = ReleaseNotes.FromJsonFile(releaseNotesFileName),
                        Architecture = architecture,
                        Os = os,
                        Version = new ProductVersion(version),
                        Identifier = productName
                    };
                }

                //The artifact directory (thus the specified artifact) does not exist.
                return null;
            }
            catch (Exception e)
            {
                logger.LogCritical(e.Message);
                throw;
            }
        }

        public ArtifactDownload GetSpecificArtifact(string productName, string os, string architecture,
            string version)
        {
            try
            {
                var path = GenerateArtifactPath(productName, os, architecture, version);

                if (path != null && Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();

                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);

                    return new ArtifactDownload
                    {
                        Payload = File.ReadAllBytes(ResolveFileInDirectory(path, deploymentMetaInfo.ArtifactFileName)),
                        FileName = deploymentMetaInfo.ArtifactFileName,
                    };
                }

                logger.LogWarning("The directory {0} does not exist!", path);
                return null;
            }

            catch (Exception e)
            {
                logger.LogCritical(e.Message);
                throw;
            }
        }

        public bool DeleteSpecificArtifactIfExists(string productName, string os, string architecture, string version)
        {
            var path = GenerateArtifactPath(productName, os, architecture, version);

            if (path == null || !Directory.Exists(path))
                return false;

            Directory.Delete(path, true);

            return true;
        }

        public bool DeleteProductIfExists(string productName)
        {
            var path = ResolveInsideRoot(productName);

            if (path == null || StorageNames.IsInternalDirectory(productName) || !Directory.Exists(path))
                return false;

            Directory.Delete(path, true);

            return true;
        }

        public List<ProductVersion> GetVersions(string productName, string os, string architecture)
        {
            var versions =
                from productDir in EnumerateProductDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                where osDir.Name == os
                from hwArchDir in osDir.EnumerateDirectories()
                where hwArchDir.Name == architecture
                from versionDir in hwArchDir.EnumerateDirectories()
                select StorageNames.TryParseVersion(versionDir.Name, out var version) ? version : null;

            // Directories that are not named like a version are skipped.
            return versions.Where(v => v != null).OrderByDescending(v => v).ToList();
        }

        public List<string> GetPlatforms(string productName, string version)
        {
            var platforms =
                from productDir in EnumerateProductDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                where versionDir.Name == version
                select osDir.Name + "-" + hwArchDir.Name;

            return platforms.OrderBy(p => p).ToList();
        }

        public BackupInformation RunBackup()
        {
            var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

            string backupFileName = "backup_" + timeStamp + ".zip";
            string backupArchiveFileName = Path.Combine(backupRootDir.ToString(), backupFileName);

            //Clear the backup folder first
            if (backupRootDir.Exists)
            {
                backupRootDir.DeleteContent();
            }
            else
            {
                backupRootDir.Create();
            }

            //Create the backup -> zip the whole ArtifactRoot folder
            ZipFile.CreateFromDirectory(artifactRootDir.ToString(), backupArchiveFileName);

            return new BackupInformation
            {
                FullPath = backupArchiveFileName,
                FileName = backupFileName
            };
        }

        public void RestoreBackup(ZipArchive backupPayload)
        {
            try
            {
                //Clear the whole artifact root directory or create it, if it's not existing
                if (artifactRootDir.Exists)
                {
                    artifactRootDir.DeleteContent();
                }
                else
                {
                    artifactRootDir.Create();
                }

                backupPayload.ExtractToDirectory(artifactRootDir.ToString());
            }
            catch (Exception e)
            {
                logger.LogCritical("unexpected error during restoring the backup: {message}", e.Message);
                throw;
            }
        }
        
        #endregion
        
        #region ---------- Private methods ----------

        /// <summary>
        /// Builds the path of an artifact directory. Returns null if the path would be outside of the
        /// artifact root directory (e.g. because of ".." in a name).
        /// </summary>
        private string GenerateArtifactPath(string product, string os, string architecture, string version)
        {
            return ResolveInsideRoot(product, os, architecture, version);
        }

        private string ResolveInsideRoot(params string[] segments)
        {
            if (segments.Any(string.IsNullOrEmpty))
                return null;

            var root = Path.GetFullPath(artifactRootDir.FullName);
            var path = Path.GetFullPath(Path.Combine(new[] { root }.Concat(segments).ToArray()));
            var rootWithSeparator = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;

            // Every segment has to be a real sub directory (no "..", no absolute paths, no separators).
            var expectedDepth = segments.Length;
            if (!path.StartsWith(rootWithSeparator, StringComparison.Ordinal)
                || Path.GetRelativePath(root, path).Split(Path.DirectorySeparatorChar).Length != expectedDepth)
            {
                logger.LogWarning("Rejected path outside of the artifact directory: {0}", string.Join("/", segments));
                return null;
            }

            return path;
        }

        /// <summary>
        /// Resolves a file name from a deployment.json inside the artifact directory. Throws if the file
        /// name points outside of the directory.
        /// </summary>
        private static string ResolveFileInDirectory(string directory, string fileName)
        {
            if (!StorageNames.IsValidFileName(fileName))
                throw new InvalidDataException($"Invalid file name in {StorageNames.DeploymentMetaFileName}: {fileName}");

            return Path.Combine(directory, fileName);
        }

        private string GenerateTemporaryPath()
        {
            return Path.Combine(artifactRootDir.ToString(), TemporaryDirectoryName, Guid.NewGuid().ToString());
        }

        private void CleanupTemporaryDirectory(DirectoryInfo tmpDir)
        {
            try
            {
                tmpDir.Refresh();
                if (tmpDir.Exists)
                    tmpDir.Delete(true);

                var parent = tmpDir.Parent;
                if (parent != null && parent.Exists && !parent.EnumerateFileSystemInfos().Any())
                    parent.Delete();
            }
            catch (IOException e)
            {
                logger.LogWarning("Could not remove the temporary directory {0}: {1}", tmpDir.FullName, e.Message);
            }
        }

        private static void EnsureValidForStorage(ReleaseArtifact artifact)
        {
            var info = artifact.DeploymentInformation;
            var meta = artifact.DeploymentMetaInformation;

            if (!StorageNames.IsValidSegment(info.Identifier)
                || !StorageNames.IsValidSegment(info.Os)
                || !StorageNames.IsValidSegment(info.Architecture)
                || info.Version == null
                || !StorageNames.IsValidVersion(info.Version.ToString())
                || !StorageNames.IsValidFileName(meta.ArtifactFileName)
                || !StorageNames.IsValidFileName(meta.ReleaseNotesFileName))
            {
                throw new ArgumentException("The artifact contains names that are not allowed in the artifact directory.");
            }
        }

        /// <summary>
        /// Enumerates the product directories (internal directories like the temporary directory are skipped).
        /// </summary>
        private IEnumerable<DirectoryInfo> EnumerateProductDirectories()
        {
            return artifactRootDir.EnumerateDirectories().Where(dir => !StorageNames.IsInternalDirectory(dir.Name));
        }

        private DeploymentInformation TryGetDeploymentInformation(string productName, string os, string architecture, string version)
        {
            try
            {
                return GetDeploymentInformation(productName, os, architecture, version);
            }
            catch (Exception e)
            {
                logger.LogWarning("Skipping the broken artifact {0}/{1}/{2}/{3}: {4}", productName, os, architecture, version, e.Message);
                return null;
            }
        }

        private DeploymentMetaInformation GetDeploymentMetaInfo(IEnumerable<FileInfo> fileInfos)
        {
            var deploymentMetaName = fileInfos.FirstOrDefault(f => f.Name == StorageNames.DeploymentMetaFileName);

            if (deploymentMetaName == null)
                throw new Exception("meta information of the specified product does not exist!");

            return
                DeploymentMetaInformation.FromJsonFile(deploymentMetaName.FullName);
        }

        public List<string> GetProductList()
        {
            var productList = EnumerateProductDirectories()
                .Select( directoryInfo => directoryInfo.Name )
                .ToList();
            return productList;
        }

        public DiskUsage GetDiskUsage()
        {
            var driveName = Path.GetPathRoot(this.artifactRootDir.FullName);
            var drive = DriveInfo.GetDrives().Where(driveInfo => driveInfo.Name == driveName).First();
            var usedDiskSpace = this.artifactRootDir.GetDiskUsage();
            return new DiskUsage() {
                TotalSize = (int)(drive.TotalSize / (1024 * 1024)),
                AvailableFreeSpace = (int)(drive.AvailableFreeSpace / (1024 * 1024)),
                UsedDiskSpace = (int)(this.artifactRootDir.GetDiskUsage() / (1024 * 1024)),
            };
        }

        public int GetNumberOfProducts()
        {
            return EnumerateProductDirectories().Count();
        }

        public int GetNumberOfArtifacts()
        {
            var artifacts =
                from productDir in EnumerateProductDirectories()
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                select versionDir;
            return artifacts.Count();
        }

        #endregion
    }
}