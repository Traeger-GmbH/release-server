//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionList.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information of the available versions of a specific artifact.
    /// </summary>
    public class ProductVersionList
    {
        /// <summary>
        /// Gets the available versions for the corresponding artifact.
        /// </summary>
        public List<ProductVersion> Versions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVersionList"/> class.
        /// </summary>
        /// <param name="productVersions">A list of <see cref="ProductVersion"/> instances.</param>
        public ProductVersionList(List<ProductVersion> productVersions)
        {
            Versions = productVersions;
        }
    }
}