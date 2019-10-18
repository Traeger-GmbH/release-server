using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Mappers;
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
        
        public async Task StoreArtifact(string version, string os, string architecture, IFormFile payload)
        {
            var artifact = ReleaseArtifactMapper.ConvertToReleaseArtifact(version, os, architecture, payload);
            await FsReleaseArtifactRepository.StoreArtifact(artifact);
        }

        public async Task<string> Get()
        {
            return "this is a test artifact";
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string version, string os, string architecture, IFormFile payload);
        Task<string> Get();
    }
}