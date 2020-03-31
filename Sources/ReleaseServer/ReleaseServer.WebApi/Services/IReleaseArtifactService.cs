using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Services
{
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        Task<List<ProductInformationModel>> GetProductInfos(string productName);
        Task<List<string>> GetPlatforms(string productName, string version);
        Task<string> GetReleaseInfo(string productName, string os, string architecture, string version);
        Task<List<string>> GetVersions(string productName, string os, string architecture);
        Task<string> GetLatestVersion(string productName, string os, string architecture);
        Task<ArtifactDownloadModel> GetSpecificArtifact(string productName, string os, string architecture, string version);
        Task<ArtifactDownloadModel> GetLatestArtifact(string productName, string os, string architecture);
        Task<bool> DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        Task<bool> DeleteProduct(string productName);
        Task<BackupInformationModel> RunBackup();
        Task RestoreBackup(IFormFile payload);
    }
}
