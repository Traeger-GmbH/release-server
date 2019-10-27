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

        public string GetReleaseInfo(string product, string os, string architecture, string version)
        {
            return FsReleaseArtifactRepository.GetReleaseInfo(product, os, architecture, version);
        }

        public async Task<string> Get()
        {
            return "this is a test artifact";
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        List<ProductInformationModel> GetProductInfos(string productName);
        List<string> GetPlatforms(string productName, string version);
        string GetReleaseInfo(string product, string os, string architecture, string version);
        Task<string> Get();
    }
}