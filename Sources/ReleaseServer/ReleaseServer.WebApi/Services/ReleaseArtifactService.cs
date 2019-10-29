using System;
using System.Collections.Generic;
using System.IO.Compression;
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

        public FsReleaseArtifactService()
        {
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository();
        }
        
        public async Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload)
        {
            var artifact = ReleaseArtifactMapper.ConvertToReleaseArtifact(product, os, architecture, version, payload);
            await FsReleaseArtifactRepository.StoreArtifact(artifact);
        }
        public List<ProductInformationModel> GetProductInfos(string productName)
        {
            return FsReleaseArtifactRepository.GetInfosByProductName(productName);
        }

        public List<string> GetPlatforms(string productName, string version)
        {
            var productInfos = FsReleaseArtifactRepository.GetInfosByProductName(productName);            
            var relevantProductInfos = from productInfo in productInfos
                where productInfo.Version == version
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

            return relevantProductInfos.Select(relevantProductInfo => relevantProductInfo.Version).ToList();
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
        byte[] GetSpecificArtifact(string productName, string os, string architecture, string version);
    }
}