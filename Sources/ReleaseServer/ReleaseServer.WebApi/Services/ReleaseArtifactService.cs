using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;

namespace release_server_web_api.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private ILogger Logger;

        public FsReleaseArtifactService( IReleaseArtifactRepository fsReleaseArtifactRepository, ILogger<FsReleaseArtifactService> logger)
        {
            FsReleaseArtifactRepository = fsReleaseArtifactRepository;
            Logger = logger;
        }
        
        public async Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload)
        {

            using (var zipMapper = new ZipArchiveMapper())
            {
                Logger.LogDebug("convert the uploaded payload to a ZIP archive");
                var zipPayload = zipMapper.FormFileToZipArchive(payload);
                
                var artifact = ReleaseArtifactMapper.ConvertToReleaseArtifact(product, os, architecture, version, zipPayload);
                await FsReleaseArtifactRepository.StoreArtifact(artifact);
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
            FsReleaseArtifactRepository.DeleteSpecificArtifact(productName, os, architecture, version);
        }

        public void DeleteProduct(string productName)
        {
            FsReleaseArtifactRepository.DeleteProduct(productName);
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        List<ProductInformationModel> GetProductInfos(string productName);
        List<string> GetPlatforms(string productName, string version);
        string GetReleaseInfo(string productName, string os, string architecture, string version);
        List<string> GetVersions(string productName, string os, string architecture);
        string GetLatestVersion(string productName, string os, string architecture);
        ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version);
        ArtifactDownloadModel GetLatestArtifact(string productName, string os, string architecture);
        void DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteProduct(string productName);
    }
}