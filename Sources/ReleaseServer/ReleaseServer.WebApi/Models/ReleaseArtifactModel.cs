using Microsoft.AspNetCore.Http;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseArtifactModel
    {
        public ProductInformationModel ProductInformation { get; set; }
        
        //TODO: Find a better solution for IFormFile
        public IFormFile Payload { get; set; }
    }
}