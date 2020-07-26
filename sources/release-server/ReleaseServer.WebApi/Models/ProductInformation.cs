//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformation.cs" company="Traeger IndustryComponents GmbH">
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
        public string Identifier { get; set; }
        
        /// <summary>
        /// Gets or sets the product version of the artifact.
        /// </summary>
        public ProductVersion Version { get; set; }
        
        /// <summary>
        /// Gets or sets the operating system of the artifact.
        /// </summary>
        public string Os { get; set; }
        
        /// <summary>
        /// Gets or sets the hardware architecture of the artifact.
        /// </summary>
        public string Architecture { get; set; }
        
        /// <summary>
        /// Gets or sets the release notes of the artifact.
        /// </summary>
        public ReleaseNotes ReleaseNotes { get; set; }
    }
}