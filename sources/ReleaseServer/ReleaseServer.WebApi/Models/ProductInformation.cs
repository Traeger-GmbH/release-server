//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides product information about a specific artifact.
    /// </summary>
    public class ProductInformation
    {
        /// <summary>
        /// Gets or sets the product identifier (product name) of the artifact.
        /// </summary>
        /// <value>The product name of the artifact (e.g. softareX).</value>
        public string Identifier { get; set; }
        
        /// <summary>
        /// Gets or sets the product version of the artifact.
        /// </summary>
        /// <value>The product version of the artifact (e.g. 1.0, 1.1-beta)</value>
        public ProductVersion Version { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system of the artifact.
        /// </summary>
        /// <value>The operating system of the artifact (e.g. ubuntu, windows)</value>
        public string Os { get; set; }
        
        /// <summary>
        /// Gets or sets the hardware architecture of the artifact.
        /// </summary>
        /// <value>The hardware architecture of the artifact (e.g. arm64, amd64).</value>
        public string Architecture { get; set; }
        
        /// <summary>
        /// Gets or sets the release notes of the artifact.
        /// </summary>
        /// <value>The release notes of the artifact.</value>
        public ReleaseNotes ReleaseNotes { get; set; }
    }
}