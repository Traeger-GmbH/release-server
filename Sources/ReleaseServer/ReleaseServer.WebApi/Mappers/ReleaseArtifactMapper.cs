using System.IO.Compression;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifactModel ConvertToReleaseArtifact(string product, string os, string architecture,
            string version, ZipArchive payload)
        {
            return new ReleaseArtifactModel
            {
                ProductInformation = new ProductInformationModel
                {
                    ProductIdentifier = product,
                    Os = os,
                    HwArchitecture = architecture,
                    Version = version
                },
                Payload = payload
            };
        }
    }
}