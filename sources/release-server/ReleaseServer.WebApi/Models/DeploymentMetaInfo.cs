//--------------------------------------------------------------------------------------------------
// <copyright file="DeploymentMetaInfo.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

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