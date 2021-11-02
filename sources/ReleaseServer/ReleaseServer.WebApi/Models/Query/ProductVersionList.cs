//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionList.cs" company="Traeger Industry Components GmbH">
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
        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVersionList"/> class.
        /// </summary>
        /// <param name="productVersions">A list of <see cref="ProductVersion"/> instances.</param>
        public ProductVersionList(List<ProductVersion> productVersions)
        {
            Versions = productVersions;
        }
        
        #endregion
        
        #region ---------- Public properties ----------
        
        /// <summary>
        /// Gets the available versions for the corresponding artifact.
        /// </summary>
        /// <value>All available versions for the specified artifact. It's empty, if there exists no artifact with the
        /// specified criteria.</value>
        public List<ProductVersion> Versions { get; }
        
        #endregion
        
    }
}