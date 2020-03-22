using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        public FsReleaseArtifactService( IReleaseArtifactRepository fsReleaseArtifactRepository, ILogger<FsReleaseArtifactService> logger)
        {
            FsReleaseArtifactRepository = fsReleaseArtifactRepository;
            Logger = logger;
            DirectoryLock = new SemaphoreSlim(1,1);
        }
        
        public void StoreArtifact(string product, string os, string architecture, string version, IFormFile payload)
        {
            using (var zipMapper = new ZipArchiveMapper())
            {
                Logger.LogDebug("convert the uploaded payload to a ZIP archive");
                var zipPayload = zipMapper.FormFileToZipArchive(payload);
                
                var artifact = ReleaseArtifactMapper.ConvertToReleaseArtifact(product, os, architecture, version, zipPayload);

                lock (DirectoryLock)
                    FsReleaseArtifactRepository.StoreArtifact(artifact);
            }
        }
        public List<ProductInformationModel> GetProductInfos(string productName)
        {
            return FsReleaseArtifactRepository.GetInfosByProductName(productName);
        }

        public List<string> GetPlatforms(string productName, string version)
        {
            return FsReleaseArtifactRepository.GetPlatforms(productName, version);
        }

        public string GetReleaseInfo(string productName, string os, string architecture, string version)
        {
            return FsReleaseArtifactRepository.GetReleaseInfo(productName, os, architecture, version);
        }

        public List<string> GetVersions(string productName, string os, string architecture)
        {
            return FsReleaseArtifactRepository.GetVersions(productName, os, architecture);
        }

        public string GetLatestVersion(string productName, string os, string architecture)
        {
            var versions = FsReleaseArtifactRepository.GetVersions(productName, os, architecture);
            
            return versions.First();
        }

        public ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version)
        {
           return FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version);
        }

        public ArtifactDownloadModel GetLatestArtifact(string productName, string os, string architecture)
        {
            var latestVersion = GetLatestVersion(productName, os, architecture);

            return FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, latestVersion);
        }

        public void DeleteSpecificArtifact(string productName, string os, string architecture, string version)
        {
            lock(DirectoryLock)
                FsReleaseArtifactRepository.DeleteSpecificArtifact(productName, os, architecture, version);
        }

        public void DeleteProduct(string productName)
        {
            lock (DirectoryLock)
                FsReleaseArtifactRepository.DeleteProduct(productName);
        }

        public BackupInformationModel RunBackup()
        {
            lock (DirectoryLock)
                return  FsReleaseArtifactRepository.RunBackup();
        }

        public void RestoreBackup(IFormFile payload)
        {
            using (var zipMapper = new ZipArchiveMapper())
            {
                Logger.LogDebug("convert the uploaded backup payload to a ZIP archive");
                var zipPayload = zipMapper.FormFileToZipArchive(payload);
                
                lock (DirectoryLock)
                    FsReleaseArtifactRepository.RestoreBackup(zipPayload);
            }
        }
    }
}