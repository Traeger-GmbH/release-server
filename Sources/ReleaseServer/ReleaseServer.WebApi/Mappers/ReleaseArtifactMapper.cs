using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifactModel ConvertToReleaseArtifact(string version, string os, string architecture,
            IFormFile payload)
        {
            var productInformation = new ProductInformationModel
            {
                Version = version,
                Os = os,
                HwArchitecture = architecture
            };

            return new ReleaseArtifactModel
            {
                ProductInformation = productInformation,
                Payload = payload
            };
        }
    }
}