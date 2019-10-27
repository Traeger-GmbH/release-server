using System.Collections.Generic;
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

        public async Task<string> Get()
        {
            return "this is a test artifact";
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        List<ProductInformationModel> GetProductInfos(string productName);
        Task<string> Get();
    }
}