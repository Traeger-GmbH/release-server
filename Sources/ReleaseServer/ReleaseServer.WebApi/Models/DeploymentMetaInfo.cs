using System;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Common;

namespace ReleaseServer.WebApi.Models
{
    public class DeploymentMetaInfo : JsonSerializable<DeploymentMetaInfo>
    {
        [JsonRequired]
        public string ReleaseNotesFileName { get; set; }
        
        [JsonRequired]
        public string ArtifactFileName { get; set; }
        
        [JsonRequired]
        public DateTime ReleaseDate { get; set;}
    }
}