using System.IO.Compression;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseArtifact
    {
        public ProductInformation ProductInformation { get; set; }
        
        public ZipArchive Payload { get; set; }
    }
}