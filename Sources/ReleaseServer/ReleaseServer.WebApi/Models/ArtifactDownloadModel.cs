namespace ReleaseServer.WebApi.Models
{
    public class ArtifactDownloadModel
    {
        public string FileName { get; set; }
        
        public byte[] Payload { get; set; }
    }
}