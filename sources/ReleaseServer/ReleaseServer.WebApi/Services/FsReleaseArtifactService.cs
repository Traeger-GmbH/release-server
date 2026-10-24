//--------------------------------------------------------------------------------------------------
// <copyright file="FsReleaseArtifactService.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    internal class FsReleaseArtifactService : IReleaseArtifactService
    {
        
        #region ---------- Private fields ----------
        
        private IReleaseArtifactRepository fsReleaseArtifactRepository;
        private ILogger logger;
        
        #endregion
        
        #region ---------- Private synchronization fields ----------
        
        // Serializes all write operations on the artifact directory (the service is a singleton).
        private readonly SemaphoreSlim directoryLock = new SemaphoreSlim(1, 1);

        #endregion
        
        #region ---------- Public constructors ----------
        
        public FsReleaseArtifactService(IReleaseArtifactRepository fsReleaseArtifactRepository,
            ILogger<FsReleaseArtifactService> logger)
        {
            this.fsReleaseArtifactRepository = fsReleaseArtifactRepository;
            this.logger = logger;
        }

        #endregion

        #region ---------- Public methods (by IReleaseArtifactService) ----------

        public async Task<ServiceActionResult> StorePackage(IFormFile payload, bool force = false)
        {
            logger.LogDebug("Convert the uploaded payload to package");
            using var package = new Package(payload);
            ServiceActionResult result;
            
            if (package.ValidationResult.IsValid) {
                // Only the same version of the product is a conflict; new versions can be added without "force".
                var versionExists = this.fsReleaseArtifactRepository.GetInfosByProductName(package.Identifier)
                    .Any(deployment => deployment.Version == package.Version);

                if (versionExists && !force) {
                    var errorMessage = $"This package ({package.Identifier}-v{package.Version}) already exists. " +
                        $"Set the parameter \"force = true\" in order to overwrite the existing package.";
                    result = new ServiceActionResult(System.Net.HttpStatusCode.Conflict, new string[] { errorMessage });
                } else {
                    try {
                        await directoryLock.WaitAsync();

                        foreach (var (platform, deployment) in package.Deployments) {
                            var metaInformation = new DeploymentMetaInformation {
                                ArtifactFileName = deployment.Name,
                                ReleaseNotesFileName = "releaseNotes.json"
                            };

                            using var artifact = new ReleaseArtifact() {
                                DeploymentInformation = new DeploymentInformation {
                                    Identifier = package.Identifier,
                                    Version = package.Version,
                                    ReleaseNotes = package.ReleaseNotes,
                                    Architecture = platform.Architecture,
                                    Os = platform.OperatingSystem
                                },
                                DeploymentMetaInformation = metaInformation,
                                Content = deployment.Open()
                            };
                        
                            this.fsReleaseArtifactRepository.StoreArtifact(artifact);
                        }
                    }
                    finally {
                        directoryLock.Release();
                    }
                    result = new ServiceActionResult(System.Net.HttpStatusCode.OK);
                }
            }
            else {
                foreach (var error in package.ValidationResult.ValidationErrors) {
                    logger.LogError(error);
                }
                result = new ServiceActionResult(System.Net.HttpStatusCode.BadRequest, package.ValidationResult.ValidationErrors);
            }
            return result;
        }

        public async Task StoreArtifact(string productName, string os, string architecture, string version,
            IFormFile payload)
        {
            logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            using var zipPayload = new ZipArchive(payload.OpenReadStream());
            
            var artifact =
                ReleaseArtifactMapper.ConvertToReleaseArtifact(productName, os, architecture, version, zipPayload);

            await directoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                await Task.Run(() => fsReleaseArtifactRepository.StoreArtifact(artifact));
            }
            finally
            {
                directoryLock.Release();
            }
            
        }

        public async Task<List<DeploymentInformation>> GetDeploymentInformations(string productName)
        {
            return await Task.Run(() => fsReleaseArtifactRepository.GetInfosByProductName(productName));
        }

        public async Task<ProductVersion> GetLatestVersion(string productName, string os, string architecture)
        {
            var versions = await Task.Run(() => fsReleaseArtifactRepository.GetVersions(productName, os, architecture));

            if (versions.IsNullOrEmpty())
                return null;

            return versions.First();
        }

        public async Task<ArtifactDownload> GetSpecificArtifact(string productName, string os, string architecture,
            string version)
        {
            return await Task.Run(() =>
                fsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version));
        }

        public async Task<ArtifactDownload> GetLatestArtifact(string productName, string os, string architecture)
        {
            return await Task.Run(() =>
            {
                // Versions are ordered descending. Incomplete or broken artifact directories (e.g. of an
                // interrupted upload) are skipped, so the latest complete version is delivered.
                foreach (var version in fsReleaseArtifactRepository.GetVersions(productName, os, architecture))
                {
                    try
                    {
                        var artifact = fsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version.ToString());
                        if (artifact != null)
                            return artifact;
                    }
                    catch (Exception e)
                    {
                        logger.LogWarning("Skipping the broken artifact {0}/{1}/{2}/{3}: {4}", productName, os, architecture, version, e.Message);
                    }
                }

                return null;
            });
        }

        public async Task<bool> DeleteSpecificArtifactIfExists(string productName, string os, string architecture,
            string version)
        {
            await directoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() =>
                    fsReleaseArtifactRepository.DeleteSpecificArtifactIfExists(productName, os, architecture, version));
            }
            finally
            {
                directoryLock.Release();
            }
        }

        public async Task<bool> DeleteProductIfExists(string productName)
        {
            await directoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() => fsReleaseArtifactRepository.DeleteProductIfExists(productName));
            }
            finally
            {
                directoryLock.Release();
            }
        }

        public async Task<BackupInformation> RunBackup()
        {
            BackupInformation backup;

            await directoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                backup = await Task.Run(() => fsReleaseArtifactRepository.RunBackup());
            }
            finally
            {
                directoryLock.Release();
            }

            return backup;
        }

        public async Task RestoreBackup(IFormFile payload)
        {
            
            logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            ZipArchive zipPayload;
            try
            {
                zipPayload = new ZipArchive(fileStream);
            }
            catch (InvalidDataException e)
            {
                throw new BackupValidationException(new[] { "The uploaded file is not a valid ZIP archive: " + e.Message });
            }

            using var disposableZipPayload = zipPayload;
            await directoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                await Task.Run(() => fsReleaseArtifactRepository.RestoreBackup(zipPayload));
            }
            finally
            {
                directoryLock.Release();
            }
        
        }

        public ValidationResult ValidateUploadPayload(IFormFile payload)
        {
            DeploymentMetaInformation deploymentMetaInfo;
            
            logger.LogDebug("convert the uploaded payload to a ZIP archive");
            using var fileStream = payload.OpenReadStream();
            using var payloadZipArchive = new ZipArchive(payload.OpenReadStream());
            
            //Try to get the deployment.json entry from the zip archive and check, if it exists
            var deploymentInfoEntry = payloadZipArchive.GetEntry("deployment.json");

            if (deploymentInfoEntry == null)
            {
                var validationError = "the deployment.json does not exist in the uploaded payload!";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }

            //Open the deployment.json and extract the DeploymentMetaInfo of it
            using (StreamReader deploymentInfoFile = new StreamReader(deploymentInfoEntry.Open(), System.Text.Encoding.UTF8))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    deploymentMetaInfo = (DeploymentMetaInformation) serializer.Deserialize(deploymentInfoFile, typeof(DeploymentMetaInformation));
                }
                catch (Exception e)
                {
                    var validationError = "the deployment meta information (deployment.json) is invalid! "
                                          + "Error: " + e.Message;
                    logger.LogError(validationError);
                    return new ValidationResult(false, validationError);
                }
            }

            if (deploymentMetaInfo == null)
            {
                var validationError = "the deployment meta information (deployment.json) is empty!";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }

            //Check the file names, they are used as file names in the artifact directory
            if (!StorageNames.IsValidFileName(deploymentMetaInfo.ArtifactFileName)
                || !StorageNames.IsValidFileName(deploymentMetaInfo.ReleaseNotesFileName)
                || deploymentMetaInfo.ArtifactFileName == StorageNames.DeploymentMetaFileName
                || deploymentMetaInfo.ReleaseNotesFileName == StorageNames.DeploymentMetaFileName
                || deploymentMetaInfo.ArtifactFileName == deploymentMetaInfo.ReleaseNotesFileName
                || StorageNames.IsOperatingSystemMetadata(deploymentMetaInfo.ArtifactFileName)
                || StorageNames.IsOperatingSystemMetadata(deploymentMetaInfo.ReleaseNotesFileName))
            {
                var validationError = "the file names in the deployment meta information (deployment.json) are invalid! "
                                      + "They must be different plain file names without path separators.";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }

            //Check if the uploaded payload contains the the artifact file
            if (payloadZipArchive.GetEntry(deploymentMetaInfo.ArtifactFileName) == null)
            {
                var validationError = "the expected artifact" + " \"" + deploymentMetaInfo.ArtifactFileName +
                                      "\"" + " does not exist in the uploaded payload!";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }

            if (payloadZipArchive.GetEntry(deploymentMetaInfo.ReleaseNotesFileName) == null)
            {
                var validationError = "the expected release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                      "\"" +  " does not exist in the uploaded payload!";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }
            
            //Check if the uploaded payload contains the release notes file
            var releaseNotesEntry = payloadZipArchive.GetEntry(deploymentMetaInfo.ReleaseNotesFileName);
            
            if (releaseNotesEntry == null)
            {
                var validationError = "the expected release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                      "\"" +  " does not exist in the uploaded payload!";
                logger.LogError(validationError);
                return new ValidationResult(false, validationError);
            }
            
            //Try to Deserialize the release notes and check if it is valid
            using (StreamReader releaseNotesFile = new StreamReader(releaseNotesEntry.Open(), System.Text.Encoding.UTF8))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Deserialize(releaseNotesFile, typeof(ReleaseNotes));
                }
                catch (Exception e)
                {
                    var validationError = "the release notes file" + " \"" +  deploymentMetaInfo.ReleaseNotesFileName +
                                          "\" is an invalid json file! " + "Error: " + e.Message;;
                    return new ValidationResult(false, validationError);
                }
            }
            
            //The uploaded payload is valid
            return new ValidationResult(true);
        }

        public async Task<List<string>> GetProductList()
        {
            return await Task.Run(() =>
                fsReleaseArtifactRepository.GetProductList());
        }

        public async Task<Statistics> GetStatistics()
        {
            return await Task.Run(() => {
                var diskUsage = this.fsReleaseArtifactRepository.GetDiskUsage();
                var numberOfProducts = this.fsReleaseArtifactRepository.GetNumberOfProducts();
                var numberOfArtifacts= this.fsReleaseArtifactRepository.GetNumberOfArtifacts();
                return new Statistics() {
                    Disk = diskUsage,
                    NumberOfProducts = numberOfProducts,
                    NumberOfArtifacts = numberOfArtifacts
                };
            });
        }

        #endregion
    }
}
