//--------------------------------------------------------------------------------------------------
// <copyright file="IReleaseArtifactRepository.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO.Compression;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    public interface IReleaseArtifactRepository     
    {
        void StoreArtifact(ReleaseArtifact artifact);
        List<ProductInformation> GetInfosByProductName(string productName);
        ReleaseInformation GetReleaseInfo(string product, string os, string architecture, string version);
        ArtifactDownload GetSpecificArtifact(string productName, string os, string architecture, string version);
        bool DeleteSpecificArtifactIfExists(string productName, string os, string architecture, string version);
        bool DeleteProductIfExists(string productName);
        List<ProductVersion> GetVersions(string productName, string os, string architecture);
        List<string> GetPlatforms(string productName, string version);
        BackupInformation RunBackup();
        void RestoreBackup(ZipArchive backupPayload);
    }
}