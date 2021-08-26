//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifact.cs" company="Traeger Industry Components GmbH">
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
        /// <value>The product information of the artifact in form of an <see cref="DeploymentInformation"/> object.</value>
        public DeploymentInformation DeploymentInformation { get; set; }
        
        /// <summary>
        /// Gets or sets the payload of the artifact in form of a <see cref="ZipArchive"/>.
        /// </summary>
        /// <value>The payload of the release artifact artifact in form of a <see cref="ZipArchive"/>.</value>
        public ZipArchive Payload { get; set; }
    }
}