using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Services
{
    public interface IReleaseArtifactService
    {
        Task StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        Task<List<ProductInformation>> GetProductInfos(string productName);
        Task<List<string>> GetPlatforms(string productName, string version);
        Task<ReleaseInformation> GetReleaseInfo(string productName, string os, string architecture, string version);
        Task<List<ProductVersion>> GetVersions(string productName, string os, string architecture);
        Task<ProductVersion> GetLatestVersion(string productName, string os, string architecture);
        Task<ArtifactDownload> GetSpecificArtifact(string productName, string os, string architecture, string version);
        Task<ArtifactDownload> GetLatestArtifact(string productName, string os, string architecture);
        Task<bool> DeleteSpecificArtifactIfExists(string productName, string os, string architecture, string version);
        Task<bool> DeleteProductIfExists(string productName);
        Task<BackupInformation> RunBackup();
        Task RestoreBackup(IFormFile payload);
        ValidationResult ValidateUploadPayload(IFormFile payload);
    }
}
