using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Services
{
    public interface IReleaseArtifactService
    {
        void StoreArtifact(string product, string os, string architecture, string version, IFormFile payload);
        List<ProductInformationModel> GetProductInfos(string productName);
        List<string> GetPlatforms(string productName, string version);
        string GetReleaseInfo(string productName, string os, string architecture, string version);
        List<string> GetVersions(string productName, string os, string architecture);
        string GetLatestVersion(string productName, string os, string architecture);
        ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version);
        ArtifactDownloadModel GetLatestArtifact(string productName, string os, string architecture);
        void DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteProduct(string productName);
        BackupInformationModel RunBackup();
        void RestoreBackup(IFormFile payload);
    }
}