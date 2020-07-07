//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactDownload.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    public class ArtifactDownload
    {
        public string FileName { get; set; }
        
        public byte[] Payload { get; set; }
    }
}