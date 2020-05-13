using System;

namespace ReleaseServer.WebApi.Config
{
    public class DeploymentMetaInfoModel
    {
        public string ReleaseNotesFileName { get; set; }
        
        public string ArtifactFileName { get; set; }
        
        public DateTime ReleaseDate { get; set;}
    }
}