//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifact.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO.Compression;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseArtifact
    {
        public ProductInformation ProductInformation { get; set; }
        
        public ZipArchive Payload { get; set; }
    }
}