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

                //Cleanup the tmp directory
                tmpDir.Parent.Delete(true);

            }
            catch (Exception e)
            {
                logger.LogCritical(e.Message);
                throw;
            }
        }

        public List<DeploymentInformation> GetInfosByProductName(string productName)
        {
            var productInformation =
                from productDir in artifactRootDir.EnumerateDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                select GetDeploymentInformation(productDir.Name, osDir.Name, hwArchDir.Name, versionDir.Name);

            return productInformation.ToList();
        }

        public DeploymentInformation GetDeploymentInformation(string productName, string os, string architecture, string version)
        {
            try
            {
                var path = GenerateArtifactPath(productName, os, architecture, version);

                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();

                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);

                    var releaseNotesFileName = Path.Combine(path, deploymentMetaInfo.ReleaseNotesFileName);

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

                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();

                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);


                    return new ArtifactDownload
                    {
                        Payload = File.ReadAllBytes(Path.Combine(path, deploymentMetaInfo.ArtifactFileName)),
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

            if (!Directory.Exists(path))
                return false;

            Directory.Delete(path, true);

            return true;
        }

        public bool DeleteProductIfExists(string productName)
        {
            var path = Path.Combine(artifactRootDir.ToString(), productName);

            if (!Directory.Exists(path))
                return false;

            Directory.Delete(path, true);

            return true;
        }

        public List<ProductVersion> GetVersions(string productName, string os, string architecture)
        {
            var versions =
                from productDir in artifactRootDir.EnumerateDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                where osDir.Name == os
                from hwArchDir in osDir.EnumerateDirectories()
                where hwArchDir.Name == architecture
                from versionDir in hwArchDir.EnumerateDirectories()
                select new ProductVersion(versionDir.Name);

            return versions.OrderByDescending(v => v).ToList();
        }

        public List<string> GetPlatforms(string productName, string version)
        {
            var platforms =
                from productDir in artifactRootDir.EnumerateDirectories()
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

        private string GenerateArtifactPath(string product, string os, string architecture, string version)
        {
            return Path.Combine(artifactRootDir.ToString(), product, os, architecture, version);
        }

        private string GenerateTemporaryArtifactPath(DirectoryInfo temporaryDirectory, string product, string os, string architecture, string version)
        {
            return Path.Combine(temporaryDirectory.ToString(), product, os, architecture, version);
        }

        private string GenerateTemporaryPath()
        {
            return Path.Combine(artifactRootDir.ToString(), "temp", Guid.NewGuid().ToString());
        }

        private DeploymentMetaInformation GetDeploymentMetaInfo(IEnumerable<FileInfo> fileInfos)
        {
            var deploymentMetaName = fileInfos.FirstOrDefault(f => f.Name == "deployment.json");

            if (deploymentMetaName == null)
                throw new Exception("meta information of the specified product does not exist!");

            return
                DeploymentMetaInformation.FromJsonFile(deploymentMetaName.FullName);
        }

        public List<string> GetProductList()
        {
            var productList = this.artifactRootDir
                .EnumerateDirectories()
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
            return this.artifactRootDir.EnumerateDirectories().Count();
        }

        public int GetNumberOfArtifacts()
        {
            var artifacts =
                from productDir in artifactRootDir.EnumerateDirectories()
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                select versionDir;
            return artifacts.Count();
        }

        #endregion
    }
}