//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformationList.cs" company="Traeger IndustryComponents GmbH">
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
        /// <summary>
        /// Gets the a list of product information for several artifacts. 
        /// </summary>
        public List<ProductInformation> ProductInformation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInformationList"/> class.
        /// </summary>
        /// <param name="productInfoList">A list of <see cref="ProductInformation"/> instances.</param>
        public ProductInformationList(List<ProductInformation> productInfoList)
        {
            ProductInformation = productInfoList;
        }
    }
}