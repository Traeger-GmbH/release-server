using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Common;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;

namespace ReleaseServer.WebApi.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private ILogger Logger;
        private static SemaphoreSlim DirectoryLock;

        public FsReleaseArtifactService(IReleaseArtifactRepository fsReleaseArtifactRepository,
            ILogger<FsReleaseArtifactService> logger)
        {
            FsReleaseArtifactRepository = fsReleaseArtifactRepository;
            Logger = logger;
            DirectoryLock = new SemaphoreSlim(1, 1);
        }

        public async Task StoreArtifact(string product, string os, string architecture, string version,
            IFormFile payload)
        {
            using (var zipMapper = new ZipArchiveMapper())
            {
                Logger.LogDebug("convert the uploaded payload to a ZIP archive");
                var zipPayload = zipMapper.FormFileToZipArchive(payload);

                var artifact =
                    ReleaseArtifactMapper.ConvertToReleaseArtifact(product, os, architecture, version, zipPayload);

                await DirectoryLock.WaitAsync();

                //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
                try
                {
                    await Task.Run(() => FsReleaseArtifactRepository.StoreArtifact(artifact));
                }
                finally
                {
                    DirectoryLock.Release();
                }
            }
        }

        public async Task<List<ProductInformationModel>> GetProductInfos(string productName)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetInfosByProductName(productName));
        }

        public async Task<List<string>> GetPlatforms(string productName, string version)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetPlatforms(productName, version));
        }

        public async Task<ReleaseInformationModel> GetReleaseInfo(string productName, string os, string architecture, string version)
        {
            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetReleaseInfo(productName, os, architecture, version));
        }

        public async Task<List<string>> GetVersions(string productName, string os, string architecture)
        {
            return await Task.Run(() => FsReleaseArtifactRepository.GetVersions(productName, os, architecture));
        }

        public async Task<string> GetLatestVersion(string productName, string os, string architecture)
        {
            var versions = await Task.Run(() => FsReleaseArtifactRepository.GetVersions(productName, os, architecture));

            if (versions.IsNullOrEmpty())
                return null;

            return versions.First();
        }

        public async Task<ArtifactDownloadModel> GetSpecificArtifact(string productName, string os, string architecture,
            string version)
        {
            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version));
        }

        public async Task<ArtifactDownloadModel> GetLatestArtifact(string productName, string os, string architecture)
        {
            var latestVersion = await Task.Run(() => GetLatestVersion(productName, os, architecture));

            if (latestVersion == null)
                return null;

            return await Task.Run(() =>
                FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, latestVersion));
        }

        public async Task<bool> DeleteSpecificArtifactIfExists(string productName, string os, string architecture,
            string version)
        {
            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() =>
                    FsReleaseArtifactRepository.DeleteSpecificArtifactIfExists(productName, os, architecture, version));
            }
            finally
            {
                DirectoryLock.Release();
            }
        }

        public async Task<bool> DeleteProductIfExists(string productName)
        {
            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                return await Task.Run(() => FsReleaseArtifactRepository.DeleteProductIfExists(productName));
            }
            finally
            {
                DirectoryLock.Release();
            }
        }

        public async Task<BackupInformationModel> RunBackup()
        {
            BackupInformationModel backup;

            await DirectoryLock.WaitAsync();

            //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
            try
            {
                backup = await Task.Run(() => FsReleaseArtifactRepository.RunBackup());
            }
            finally
            {
                DirectoryLock.Release();
            }

            return backup;
        }

        public async Task RestoreBackup(IFormFile payload)
        {
            using (var zipMapper = new ZipArchiveMapper())
            {
                Logger.LogDebug("convert the uploaded backup payload to a ZIP archive");
                var zipPayload = zipMapper.FormFileToZipArchive(payload);

                await DirectoryLock.WaitAsync();

                //It's important to release the semaphore. try / finally block ensures a guaranteed release (also if the operation may crash) 
                try
                {
                    await Task.Run(() => FsReleaseArtifactRepository.RestoreBackup(zipPayload));
                }
                finally
                {
                    DirectoryLock.Release();
                }
            }
        }

        public async Task<ValidationResultModel> ValidateUploadPayload(IFormFile payload)
        {
            using (var zipMapper = new ZipArchiveMapper())
            {
                DeploymentMetaInfoModel deploymentMetaInfoModel;

                Logger.LogDebug("convert the uploaded payload to a ZIP archive");
                var payloadZipArchive = zipMapper.FormFileToZipArchive(payload);
                
                //Try to get the deployment.json entry from the zip archive and check, if it exists
                var deploymentInfoEntry = payloadZipArchive.GetEntry("deployment.json");

                if (deploymentInfoEntry == null)
                {
                    var validationError = "the deployment.json does not exist in the uploaded payload!";
                    Logger.LogError(validationError);
                    return new ValidationResultModel {IsValid = false, ValidationError = validationError};
                }

                //Open the deployment.json and extract the DeploymentMetaInfo of it
                var deploymentInfoStream = deploymentInfoEntry.Open();
                var deploymentInfoByteArray = await deploymentInfoStream.ToByteArrayAsync();

                //Check if the deployment.json is a valid json file
                if(!deploymentInfoByteArray.IsAValidJson(out var errorMsg))
                {
                    var validationError = "the deployment meta information (deployment.json) is an invalid json file! "
                        + "Error: " + errorMsg;
                    Logger.LogError(validationError);
                    return new ValidationResultModel {IsValid = false, ValidationError = validationError};
                }
                    
                
                //Check if the deployment.json file has a valid content
                deploymentMetaInfoModel = DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(deploymentInfoByteArray);
                if (!deploymentMetaInfoModel.IsValid())
                {
                    var validationError = "the deployment meta information (deployment.json) is invalid!";
                    Logger.LogError(validationError);
                    return new ValidationResultModel {IsValid = false, ValidationError = validationError};
                }

                //Check if the uploaded payload contains the rest of the expected parts
                if (payloadZipArchive.GetEntry(deploymentMetaInfoModel.ArtifactFileName) == null)
                {
                    var validationError = "the expected artifact" + " \"" + deploymentMetaInfoModel.ArtifactFileName +
                                          "\"" + " does not exist in the uploaded payload!";
                    Logger.LogError(validationError);
                    return new ValidationResultModel {IsValid = false, ValidationError = validationError};
                }

                if (payloadZipArchive.GetEntry(deploymentMetaInfoModel.ChangelogFileName) == null)
                {
                    var validationError = "the expected changelog file" + " \"" +  deploymentMetaInfoModel.ChangelogFileName +
                                          "\"" +  " does not exist in the uploaded payload!";
                    Logger.LogError(validationError);
                    return new ValidationResultModel {IsValid = false, ValidationError = validationError};
                }

                //The uploaded payload is valid
                return new ValidationResultModel {IsValid = true};
            }
        }
    }
}
