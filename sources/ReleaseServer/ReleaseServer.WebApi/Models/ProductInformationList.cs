//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformationList.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

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
        public ProductInformationList(IEnumerable<ProductInformation> productInfoList)
            : this(productInfoList, productInfoList.Count(), 0)
        {
            ProductInformation = productInfoList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productInfoList"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        public ProductInformationList(IEnumerable<ProductInformation> productInfoList, int limit, int offset)
        {
            this.ProductInformation = productInfoList.Skip(offset).Take(limit);
            this.TotalCount = productInfoList.Count();
            if (this.TotalCount > offset + limit)
            {
                this.NextOffset = offset + limit;
            }
        }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// Gets the a list of product information for several artifacts.
        /// </summary>
        /// <value>A <see cref="List{T}"/> of <see cref="ProductInformation"/> objects. It's empty, if there
        /// exists no product information for the specified product.</value>
        public IEnumerable<ProductInformation> ProductInformation { get; }

        /// <summary>
        /// 
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// 
        /// </summary>
        public int? NextOffset { get; }
        
        #endregion
        
    }
}