//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifact.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO.Compression;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the content of a single artifact.
    /// </summary>
    public class ReleaseArtifact
    {
        /// <summary>
        /// Gets or sets the product information of the artifact.
        /// </summary>
        public ProductInformation ProductInformation { get; set; }
        
        /// <summary>
        /// Gets or sets the payload of the artifact in form of a <see cref="ZipArchive"/>.
        /// </summary>
        public ZipArchive Payload { get; set; }
    }
}