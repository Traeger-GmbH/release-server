//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactDownload.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the meta file name and payload of the downloaded artifact.
    /// </summary>
    public class ArtifactDownload
    {
        /// <summary>
        /// Gets or sets the name of the downloaded artifact file.
        /// </summary>
        public string FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the payload of the downloaded artifact file (contains the artifact incl. meta information).
        /// </summary>
        public byte[] Payload { get; set; }
    }
}