using System;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    public class DeploymentMetaInfo
    {
        [JsonRequired]
        public string ChangelogFileName { get; set; }
        
        [JsonRequired]
        public string ArtifactFileName { get; set; }
        
        [JsonRequired]
        public DateTime ReleaseDate { get; set;}
    }
}