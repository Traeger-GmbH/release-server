using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ReleaseServer.WebApi.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Repositories
{
    public class FsReleaseArtifactRepository : IReleaseArtifactRepository
    {
        private readonly DirectoryInfo ArtifactRootDir, BackupRootDir;
        private ILogger Logger;

        public FsReleaseArtifactRepository(
            ILogger<FsReleaseArtifactRepository> logger,
            DirectoryInfo artifactDirectory,
            DirectoryInfo backupDirectory)
        {
            ArtifactRootDir = artifactDirectory;
            BackupRootDir = backupDirectory;
            Logger = logger;
        }

        public void StoreArtifact(ReleaseArtifactModel artifact)
        {

            var artifactPath = GenerateArtifactPath(
                artifact.ProductInformation.ProductIdentifier,
                artifact.ProductInformation.Os,
                artifact.ProductInformation.HwArchitecture,
                artifact.ProductInformation.Version.ToString());

            var tmpDir = new DirectoryInfo(GenerateTemporaryPath());

            try
            {
                //Create the temporary directory
                if (!tmpDir.Exists)
                    tmpDir.Create();

                //Extract the payload to the temporary directory
                artifact.Payload.ExtractToDirectory(tmpDir.ToString());
                Logger.LogDebug("The Artifact was successfully unpacked & stored to the temp directory");

                var artifactDirectory = new DirectoryInfo(artifactPath);

                //If the directory already exists, delete the old content in there
                if (artifactDirectory.Exists)
                {
                    Logger.LogDebug("This path already exists! Old content will be deleted!");
                }
                else
                {
                    artifactDirectory.Create();
                }

                tmpDir.Move(artifactDirectory, true);

                Logger.LogInformation("The Artifact was successfully stored");

                //Cleanup the tmp directory
                tmpDir.Parent.Delete(true);

            }
            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
                throw;
            }
        }

        public List<ProductInformationModel> GetInfosByProductName(string productName)
        {
            var productInformation =
                from productDir in ArtifactRootDir.EnumerateDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                select new ProductInformationModel()
                {
                    ProductIdentifier = productDir.Name,
                    Os = osDir.Name,
                    HwArchitecture = hwArchDir.Name,
                    Version = versionDir.Name.ToProductVersion()
                };

            return productInformation.ToList();
        }

        public string GetReleaseInfo(string product, string os, string architecture, string version)
        {
            try
            {
                var path = GenerateArtifactPath(product, os, architecture, version);

                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();

                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);

                    var changelog = File.ReadAllText(Path.Combine(path, deploymentMetaInfo.ChangelogFileName));
                    return changelog;
                }

                //The artifact directory (thus the specified artifact) does not exist.
                return null;

            }
            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
                throw;
            }
        }

        public ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture,
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


                    return new ArtifactDownloadModel
                    {
                        Payload = File.ReadAllBytes(Path.Combine(path, deploymentMetaInfo.ArtifactFileName)),
                        FileName = deploymentMetaInfo.ArtifactFileName,
                    };

                }

                Logger.LogWarning("The directory {0} does not exist!", path);
                return null;
            }

            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
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
            var path = Path.Combine(ArtifactRootDir.ToString(), productName);

            if (!Directory.Exists(path))
                return false;

            Directory.Delete(path, true);

            return true;
        }

        public List<string> GetVersions(string productName, string os, string architecture)
        {
            var versions =
                from productDir in ArtifactRootDir.EnumerateDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                where osDir.Name == os
                from hwArchDir in osDir.EnumerateDirectories()
                where hwArchDir.Name == architecture
                from versionDir in hwArchDir.EnumerateDirectories()
                select versionDir.Name;

            return versions.OrderByDescending(v => v).ToList();
        }

        public List<string> GetPlatforms(string productName, string version)
        {
            var platforms =
                from productDir in ArtifactRootDir.EnumerateDirectories()
                where productDir.Name == productName
                from osDir in productDir.EnumerateDirectories()
                from hwArchDir in osDir.EnumerateDirectories()
                from versionDir in hwArchDir.EnumerateDirectories()
                where versionDir.Name == version
                select osDir.Name + "-" + hwArchDir.Name;

            return platforms.OrderBy(p => p).ToList();
        }

        public BackupInformationModel RunBackup()
        {
            var timeStamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

            string backupFileName = "backup_" + timeStamp + ".zip";
            string backupArchiveFileName = Path.Combine(BackupRootDir.ToString(), backupFileName);

            //Clear the backup folder first
            if (BackupRootDir.Exists)
            {
                BackupRootDir.DeleteContent();
            }
            else
            {
                BackupRootDir.Create();
            }

            //Create the backup -> zip the whole ArtifactRoot folder
            ZipFile.CreateFromDirectory(ArtifactRootDir.ToString(), backupArchiveFileName);

            return new BackupInformationModel
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
                if (ArtifactRootDir.Exists)
                {
                    ArtifactRootDir.DeleteContent();
                }
                else
                {
                    ArtifactRootDir.Create();
                }

                backupPayload.ExtractToDirectory(ArtifactRootDir.ToString());
            }
            catch (Exception e)
            {
                Logger.LogCritical("unexpected error during restoring the backup: {message}", e.Message);
                throw;
            }
        }

        private string GenerateArtifactPath(string product, string os, string architecture, string version)
        {
            return Path.Combine(ArtifactRootDir.ToString(), product, os, architecture, version);
        }

        private string GenerateTemporaryPath()
        {
            return Path.Combine(ArtifactRootDir.ToString(), "temp", Guid.NewGuid().ToString());
        }

        private DeploymentMetaInfo GetDeploymentMetaInfo(IEnumerable<FileInfo> fileInfos)
        {
            var deploymentMetaName = fileInfos.FirstOrDefault(f => f.Name == "deployment.json");

            if (deploymentMetaName == null)
                throw new Exception("meta information of the specified product does not exist!");

            return DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(deploymentMetaName.FullName);
        }
    }
}