//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformationList.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides a list of product information for several artifacts.
    /// </summary>
    public class ProductInformationList
    {
        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInformationList"/> class.
        /// </summary>
        /// <param name="productInfoList">A list of <see cref="ProductInformation"/> instances.</param>
        public ProductInformationList(List<ProductInformation> productInfoList)
        {
            ProductInformation = productInfoList;
        }
        
        #endregion
        
        #region ---------- Public properties ----------
        
        /// <summary>
        /// Gets the a list of product information for several artifacts. 
        /// </summary>
        /// <value>A <see cref="List{T}"/> of <see cref="ProductInformation"/> objects. It's empty, if there
        /// exists no product information for the specified product.</value>
        public List<ProductInformation> ProductInformation { get; }
        
        #endregion
        
    }
}