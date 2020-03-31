using System.Collections.Generic;
using System.IO.Compression;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Repositories
{
    public interface IReleaseArtifactRepository     
    {
        void StoreArtifact(ReleaseArtifactModel artifact);
        List<ProductInformationModel> GetInfosByProductName(string productName);
        string GetReleaseInfo(string product, string os, string architecture, string version);
        ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version);
        bool DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        bool DeleteProduct(string productName);
        List<string> GetVersions(string productName, string os, string architecture);
        List<string> GetPlatforms(string productName, string version);
        BackupInformationModel RunBackup();
        void RestoreBackup(ZipArchive backupPayload);
    }
}