using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifactModel ConvertToReleaseArtifact(string product ,string version, string os, 
            string architecture, IFormFile payload)
        {
            var productInformation = new ProductInformationModel
            {
                ProductIdentifier = product,
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