using System.IO.Compression;
using Microsoft.AspNetCore.Http;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseArtifactModel
    {
        public ProductInformationModel ProductInformation { get; set; }
        
        public ZipArchive Payload { get; set; }
    }
}