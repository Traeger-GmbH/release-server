using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseArtifactMapper
    {
        public static ReleaseArtifactModel ConvertToReleaseArtifact(string product ,string version, string os, 
            string architecture, IFormFile payload)
        {
            return new ReleaseArtifactModel
            {
                ProductInformation = new ProductInformationModel
                {
                    ProductIdentifier = product,
                    Version = version,
                    Os = os,
                    HwArchitecture = architecture
                },
                Payload = payload
            };
        }
    }
}