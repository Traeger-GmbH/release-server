using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;

namespace release_server_web_api.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;

        public FsReleaseArtifactService( IReleaseArtifactRepository fsReleaseArtifactRepository)
        {
            FsReleaseArtifactRepository = fsReleaseArtifactRepository;
        }
        
        public async Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload)
        {

            using (var zipMapper = new ZipArchiveMapper())
            {
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
            var productInfos = FsReleaseArtifactRepository.GetInfosByProductName(productName);            
            var relevantProductInfos = from productInfo in productInfos
                where productInfo.Version == new Version(version)
                select productInfo;

            return relevantProductInfos.Select(relevantProductInfo => relevantProductInfo.Os + "-" + relevantProductInfo.HwArchitecture).ToList();
        }

        public string GetReleaseInfo(string productName, string os, string architecture, string version)
        {
            return FsReleaseArtifactRepository.GetReleaseInfo(productName, os, architecture, version);
        }

        public List<string> GetVersions(string productName, string os, string architecture)
        {
            var productInfos = FsReleaseArtifactRepository.GetInfosByProductName(productName);            
            var relevantProductInfos = from productInfo in productInfos
                where productInfo.Os == os && productInfo.HwArchitecture == architecture
                select productInfo;

            return relevantProductInfos.Select(relevantProductInfo => relevantProductInfo.Version.ToString()).ToList();
        }

        public string GetLatestVersion(string productName, string os, string architecture)
        {
            var productInfos = FsReleaseArtifactRepository.GetInfosByProductName(productName);
            
            


            return "";
        }

        public byte[] GetSpecificArtifact(string productName, string os, string architecture, string version)
        {
           return FsReleaseArtifactRepository.GetSpecificArtifact(productName, os, architecture, version);
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
        byte[] GetSpecificArtifact(string productName, string os, string architecture, string version);
    }
}