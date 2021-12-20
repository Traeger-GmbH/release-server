//--------------------------------------------------------------------------------------------------
// <copyright file="ProductList.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information of the available platforms for a specific artifact.
    /// </summary>
    public class ProductList
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductList"/> class.
        /// </summary>
        /// <param name="products">The list of products.</param>
        public ProductList(List<string> products)
        {
            Products = products;
        }
        
        #endregion
        
        #region ---------- Public properties ----------
        
        /// <summary>
        /// Gets the list of products.
        /// </summary>
        public List<string> Products { get; }

        #endregion
    }
}