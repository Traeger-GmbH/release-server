//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactMapper.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO.Compression;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifact ConvertToReleaseArtifact(string product, string os, string architecture,
            string version, ZipArchive payload)
        {
            return new ReleaseArtifact
            {
                ProductInformation = new ProductInformation
                {
                    Identifier = product,
                    Os = os,
                    Architecture = architecture,
                    Version = new ProductVersion(version)
                },
                Payload = payload
            };
        }
    }
}