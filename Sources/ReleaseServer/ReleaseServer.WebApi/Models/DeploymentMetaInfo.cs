using System;

namespace ReleaseServer.WebApi.Models
{
    public class DeploymentMetaInfo
    {
        public string ChangelogFileName { get; set; }
        
        public string ArtifactFileName { get; set; }
        
        public DateTime ReleaseDate { get; set;}
    }
}