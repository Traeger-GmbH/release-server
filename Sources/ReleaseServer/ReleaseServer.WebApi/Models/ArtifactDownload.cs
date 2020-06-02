namespace ReleaseServer.WebApi.Models
{
    public class ArtifactDownload
    {
        public string FileName { get; set; }
        
        public byte[] Payload { get; set; }
    }
}