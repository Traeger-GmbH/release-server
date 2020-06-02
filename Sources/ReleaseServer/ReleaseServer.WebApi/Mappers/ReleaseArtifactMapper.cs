using System;
using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
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
                    ProductIdentifier = product,
                    Os = os,
                    HwArchitecture = architecture,
                    Version = version.ToProductVersion()
                },
                Payload = payload
            };
        }
    }
}