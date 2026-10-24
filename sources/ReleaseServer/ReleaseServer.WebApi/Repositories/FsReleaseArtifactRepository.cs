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
using System.Threading;

using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    internal class FsReleaseArtifactRepository : IReleaseArtifactRepository
    {
        #region ---------- Private constants ----------

        // Internal directory for uploads in progress (inside the artifact root, so moving is atomic).
        private const string TemporaryDirectoryName = ".temp";

        // Internal directories used while restoring a backup.
        private const string RestoreStagingPrefix = ".restore-";
        private const string RestorePreviousPrefix = ".previous-";

        // Directory used for uploads in progress by release servers before version 1.0. It may be part
        // of old backups and is not restored.
        private const string LegacyTemporaryDirectoryName = "temp";

        private const string BackupFilePrefix = "backup_";
        private const string BackupFileExtension = ".zip";
        private const string PartialFileExtension = ".partial";

        // Free space that has to remain on the disk after extracting a backup.
        private const long RestoreSpaceReserve = 100L * 1024 * 1024;

        #endregion

        #region ---------- Public constants ----------

        /// <summary>
        /// The number of backups that are kept in the backup directory by default.
        /// </summary>
        public const int DefaultBackupRetentionCount = 5;

        #endregion

        #region ---------- Private fields ----------
        
        private readonly DirectoryInfo artifactRootDir, backupRootDir;
        private readonly int backupRetentionCount;
        private ILogger logger;
        
        #endregion
        
        #region ---------- Public constructors ----------

        static FsReleaseArtifactRepository()
        {
            // Avoid skipped [JsonRequired] checks when the first files are read concurrently
            JsonConfiguration.PrepareContracts();
        }

        public FsReleaseArtifactRepository(
            ILogger<FsReleaseArtifactRepository> logger,
            DirectoryInfo artifactDirectory,
            DirectoryInfo backupDirectory,
            int backupRetentionCount = DefaultBackupRetentionCount)
        {
            artifactRootDir = artifactDirectory;
            backupRootDir = backupDirectory;
            this.backupRetentionCount = Math.Max(1, backupRetentionCount);
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

                    var filePath = ResolveFileInDirectory(path, deploymentMetaInfo.ArtifactFileName);

                    if (!File.Exists(filePath))
                        throw new FileNotFoundException("The artifact file of the specified product does not exist!", deploymentMetaInfo.ArtifactFileName);

                    return new ArtifactDownload
                    {
                        FilePath = filePath,
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

            // Remove the architecture, operating system and product directories if they are empty now, so
            // no product without releases remains
            RemoveIfEmpty(ResolveInsideRoot(productName, os, architecture));
            RemoveIfEmpty(ResolveInsideRoot(productName, os));
            RemoveIfEmpty(ResolveInsideRoot(productName));

            return true;
        }

        private void RemoveIfEmpty(string directory)
        {
            try
            {
                if (directory != null && Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            catch (IOException e)
            {
                logger.LogWarning("Could not remove the empty directory {0}: {1}", directory, e.Message);
            }
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

        public BackupInformation RunBackup()
        {
            if (!backupRootDir.Exists)
            {
                backupRootDir.Create();
            }

            string backupFileName;
            string backupArchiveFileName;

            // Backup names have a resolution of one second, never overwrite an existing backup.
            while (true)
            {
                var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
                backupFileName = BackupFilePrefix + timeStamp + BackupFileExtension;
                backupArchiveFileName = Path.Combine(backupRootDir.FullName, backupFileName);

                if (!File.Exists(backupArchiveFileName))
                    break;

                Thread.Sleep(250);
            }

            //Create the backup in a partial file first, so an interrupted backup never looks complete
            var partialFileName = backupArchiveFileName + PartialFileExtension;
            try
            {
                CreateBackupArchive(partialFileName);
                File.Move(partialFileName, backupArchiveFileName);
            }
            finally
            {
                if (File.Exists(partialFileName))
                    File.Delete(partialFileName);
            }

            ApplyBackupRetention();

            return new BackupInformation
            {
                FullPath = backupArchiveFileName,
                FileName = backupFileName
            };
        }

        public void RestoreBackup(ZipArchive backupPayload)
        {
            var id = Guid.NewGuid().ToString("N");
            var stagingDir = new DirectoryInfo(Path.Combine(artifactRootDir.FullName, RestoreStagingPrefix + id));
            var previousDir = new DirectoryInfo(Path.Combine(artifactRootDir.FullName, RestorePreviousPrefix + id));

            try
            {
                if (!artifactRootDir.Exists)
                {
                    artifactRootDir.Create();
                }

                //1. Extract the backup into a staging directory (the existing data is not touched)
                EnsureEnoughSpaceForRestore(backupPayload);
                stagingDir.Create();
                try
                {
                    // ExtractToDirectory rejects entries that would be written outside of the directory.
                    backupPayload.ExtractToDirectory(stagingDir.FullName);
                }
                catch (Exception e) when (e is IOException || e is InvalidDataException || e is UnauthorizedAccessException || e is ArgumentException)
                {
                    throw new BackupValidationException(new[] { "The backup archive cannot be extracted: " + e.Message });
                }

                //2. Verify the content before anything is replaced
                RemoveNonRestorableDirectories(stagingDir);
                var errors = ValidateBackupContent(stagingDir, out var artifactCount);
                if (errors.Count > 0)
                {
                    throw new BackupValidationException(errors);
                }

                //3. Replace the existing data by the verified data
                SwapArtifactDirectoryContent(stagingDir, previousDir);
                logger.LogInformation("The backup with {0} artifacts was restored successfully", artifactCount);
            }
            catch (BackupValidationException e)
            {
                logger.LogError("The backup was not restored, the existing data is unchanged: {0}", e.Message);
                throw;
            }
            catch (Exception e)
            {
                logger.LogCritical("unexpected error during restoring the backup: {message}", e.Message);
                throw;
            }
            finally
            {
                DeleteDirectoryIfExists(stagingDir);
            }
        }
        
        #endregion
        
        #region ---------- Private methods ----------

        /// <summary>
        /// Creates a ZIP archive of the artifact directory with the same layout as
        /// <see cref="ZipFile.CreateFromDirectory(string, string)"/> (used by previous versions), so backups
        /// stay exchangeable between versions. Internal directories are not included.
        /// </summary>
        private void CreateBackupArchive(string archiveFileName)
        {
            using var archive = ZipFile.Open(archiveFileName, ZipArchiveMode.Create);
            var root = artifactRootDir.FullName;

            foreach (var entry in artifactRootDir.EnumerateFileSystemInfos())
            {
                if (entry is DirectoryInfo directory && StorageNames.IsInternalDirectory(directory.Name))
                    continue;

                AddToArchive(archive, root, entry);
            }
        }

        private static void AddToArchive(ZipArchive archive, string root, FileSystemInfo entry)
        {
            var entryName = Path.GetRelativePath(root, entry.FullName).Replace(Path.DirectorySeparatorChar, '/');

            if (entry is FileInfo file)
            {
                archive.CreateEntryFromFile(file.FullName, entryName, CompressionLevel.Optimal);
                return;
            }

            var directory = (DirectoryInfo)entry;
            var children = directory.EnumerateFileSystemInfos().ToList();

            if (children.Count == 0)
            {
                // Empty directories are stored as directory entries (e.g. products without releases).
                archive.CreateEntry(entryName + "/");
                return;
            }

            foreach (var child in children)
            {
                AddToArchive(archive, root, child);
            }
        }

        private void ApplyBackupRetention()
        {
            var backups = backupRootDir.EnumerateFiles(BackupFilePrefix + "*" + BackupFileExtension)
                .OrderByDescending(file => file.Name, StringComparer.Ordinal)
                .ToList();

            foreach (var backup in backups.Skip(backupRetentionCount))
            {
                logger.LogInformation("Deleting the old backup {0}", backup.Name);
                backup.Delete();
            }
        }

        private void EnsureEnoughSpaceForRestore(ZipArchive backupPayload)
        {
            long requiredSpace = 0;
            foreach (var entry in backupPayload.Entries)
            {
                requiredSpace += entry.Length;
            }

            long availableSpace;
            try
            {
                availableSpace = new DriveInfo(artifactRootDir.FullName).AvailableFreeSpace;
            }
            catch (Exception e) when (e is ArgumentException || e is IOException || e is UnauthorizedAccessException)
            {
                logger.LogWarning("Could not determine the free disk space: {0}", e.Message);
                return;
            }

            if (requiredSpace + RestoreSpaceReserve > availableSpace)
            {
                throw new BackupValidationException(new[]
                {
                    $"Not enough free disk space to restore the backup ({requiredSpace / (1024 * 1024)} MB required, {availableSpace / (1024 * 1024)} MB available)."
                });
            }
        }

        private void RemoveNonRestorableDirectories(DirectoryInfo stagingDir)
        {
            foreach (var directory in stagingDir.EnumerateDirectories())
            {
                if (StorageNames.IsInternalDirectory(directory.Name) || IsLegacyTemporaryDirectory(directory))
                {
                    logger.LogWarning("The directory {0} of the backup is not restored (temporary data)", directory.Name);
                    directory.Delete(true);
                }
            }
        }

        private static bool IsLegacyTemporaryDirectory(DirectoryInfo directory)
        {
            // Previous versions used "temp/<guid>" for uploads in progress. A real product named "temp"
            // has no GUID named sub directories.
            return directory.Name == LegacyTemporaryDirectoryName
                   && directory.EnumerateFileSystemInfos().All(child => child is DirectoryInfo && Guid.TryParse(child.Name, out _));
        }

        /// <summary>
        /// Verifies an extracted backup. Every artifact directory (product/os/architecture/version) needs a
        /// readable deployment.json whose files exist inside the directory and readable release notes.
        /// Everything else is allowed, because previous versions stored more (e.g. complete extracted upload
        /// archives with sub directories like __MACOSX): unknown content is accepted unless it is unsafe.
        /// </summary>
        private static List<string> ValidateBackupContent(DirectoryInfo stagingDir, out int artifactCount)
        {
            var errors = new List<string>();
            artifactCount = 0;

            foreach (var productDir in stagingDir.EnumerateDirectories())
            {
                foreach (var osDir in productDir.EnumerateDirectories())
                {
                    foreach (var architectureDir in osDir.EnumerateDirectories())
                    {
                        foreach (var versionDir in architectureDir.EnumerateDirectories())
                        {
                            var path = $"{productDir.Name}/{osDir.Name}/{architectureDir.Name}/{versionDir.Name}";

                            if (ValidateArtifactDirectory(versionDir, path, errors))
                            {
                                artifactCount++;
                            }
                        }
                    }
                }
            }

            if (errors.Count == 0 && artifactCount == 0)
            {
                errors.Add("The backup does not contain any artifact.");
            }

            return errors;
        }

        private static bool ValidateArtifactDirectory(DirectoryInfo versionDir, string path, List<string> errors)
        {
            var errorCount = errors.Count;

            var metaFile = Path.Combine(versionDir.FullName, StorageNames.DeploymentMetaFileName);
            if (!File.Exists(metaFile))
            {
                errors.Add($"\"{path}\": {StorageNames.DeploymentMetaFileName} is missing.");
                return false;
            }

            DeploymentMetaInformation meta;
            try
            {
                meta = DeploymentMetaInformation.FromJsonFile(metaFile);
            }
            catch (Exception e)
            {
                errors.Add($"\"{path}\": {StorageNames.DeploymentMetaFileName} is invalid ({e.Message}).");
                return false;
            }

            if (meta == null
                || !TryResolveFileInDirectory(versionDir.FullName, meta.ArtifactFileName, out var artifactFile)
                || !TryResolveFileInDirectory(versionDir.FullName, meta.ReleaseNotesFileName, out var releaseNotesFile))
            {
                errors.Add($"\"{path}\": {StorageNames.DeploymentMetaFileName} contains file names outside of the directory.");
                return false;
            }

            if (!File.Exists(releaseNotesFile))
            {
                errors.Add($"\"{path}\": the release notes file \"{meta.ReleaseNotesFileName}\" is missing.");
            }
            else
            {
                try
                {
                    ReleaseNotes.FromJsonFile(releaseNotesFile);
                }
                catch (Exception e)
                {
                    errors.Add($"\"{path}\": the release notes file is invalid ({e.Message}).");
                }
            }

            if (!File.Exists(artifactFile))
            {
                errors.Add($"\"{path}\": the artifact file \"{meta.ArtifactFileName}\" is missing.");
            }

            return errors.Count == errorCount;
        }

        /// <summary>
        /// Moves the current content of the artifact directory aside, moves the staged content in and
        /// deletes the previous content afterwards. Only renames within the artifact directory are used
        /// (the artifact directory itself may be a mount point). If moving fails, the previous content
        /// is moved back.
        /// </summary>
        private void SwapArtifactDirectoryContent(DirectoryInfo stagingDir, DirectoryInfo previousDir)
        {
            previousDir.Create();
            var movedAside = new List<string>();
            var movedIn = new List<string>();

            try
            {
                foreach (var entry in artifactRootDir.EnumerateFileSystemInfos().ToList())
                {
                    if (entry is DirectoryInfo && StorageNames.IsInternalDirectory(entry.Name))
                        continue;

                    MoveEntry(entry, Path.Combine(previousDir.FullName, entry.Name));
                    movedAside.Add(entry.Name);
                }

                foreach (var entry in stagingDir.EnumerateFileSystemInfos().ToList())
                {
                    MoveEntry(entry, Path.Combine(artifactRootDir.FullName, entry.Name));
                    movedIn.Add(entry.Name);
                }
            }
            catch (Exception e)
            {
                logger.LogCritical("Restoring failed while replacing the data, the previous data is put back: {0}", e.Message);

                foreach (var name in movedIn)
                {
                    DeleteEntryIfExists(Path.Combine(artifactRootDir.FullName, name));
                }

                foreach (var name in movedAside)
                {
                    MoveEntry(new DirectoryInfo(Path.Combine(previousDir.FullName, name)).Exists
                            ? (FileSystemInfo)new DirectoryInfo(Path.Combine(previousDir.FullName, name))
                            : new FileInfo(Path.Combine(previousDir.FullName, name)),
                        Path.Combine(artifactRootDir.FullName, name));
                }

                DeleteDirectoryIfExists(previousDir);
                throw;
            }

            DeleteDirectoryIfExists(previousDir);
        }

        private static void MoveEntry(FileSystemInfo entry, string destination)
        {
            if (entry is DirectoryInfo directory)
            {
                directory.MoveTo(destination);
            }
            else
            {
                ((FileInfo)entry).MoveTo(destination);
            }
        }

        private static void DeleteEntryIfExists(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
            else if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void DeleteDirectoryIfExists(DirectoryInfo directory)
        {
            try
            {
                directory.Refresh();
                if (directory.Exists)
                    directory.Delete(true);
            }
            catch (IOException e)
            {
                logger.LogWarning("Could not remove the directory {0}: {1}", directory.FullName, e.Message);
            }
        }

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
            if (!TryResolveFileInDirectory(directory, fileName, out var path))
                throw new InvalidDataException($"Invalid file name in {StorageNames.DeploymentMetaFileName}: {fileName}");

            return path;
        }

        /// <summary>
        /// Resolves a file name from a deployment.json. Like previous versions, relative paths to files in sub
        /// directories are allowed, as long as the resolved path stays inside the artifact directory.
        /// </summary>
        private static bool TryResolveFileInDirectory(string directory, string fileName, out string path)
        {
            path = null;

            if (string.IsNullOrWhiteSpace(fileName) || fileName.IndexOf('\0') >= 0)
                return false;

            var fullDirectory = Path.GetFullPath(directory);
            var directoryWithSeparator = Path.EndsInDirectorySeparator(fullDirectory)
                ? fullDirectory
                : fullDirectory + Path.DirectorySeparatorChar;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(Path.Combine(fullDirectory, fileName));
            }
            catch (Exception e) when (e is ArgumentException || e is NotSupportedException || e is PathTooLongException)
            {
                return false;
            }

            if (!fullPath.StartsWith(directoryWithSeparator, StringComparison.Ordinal))
                return false;

            path = fullPath;
            return true;
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
        /// Checks whether a product directory contains at least one release directory
        /// (product/os/architecture/version). Empty directory skeletons, e.g. left behind by previous versions
        /// after deleting all releases, are no products.
        /// </summary>
        private static bool HasReleaseDirectories(DirectoryInfo productDir)
        {
            return productDir.EnumerateDirectories()
                .SelectMany(osDir => osDir.EnumerateDirectories())
                .Any(architectureDir => architectureDir.EnumerateDirectories().Any());
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
                .Where(HasReleaseDirectories)
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
            return EnumerateProductDirectories().Count(HasReleaseDirectories);
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