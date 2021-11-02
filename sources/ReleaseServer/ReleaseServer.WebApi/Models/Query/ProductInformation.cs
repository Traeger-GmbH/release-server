//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides a list of product information for several artifacts.
    /// </summary>
    public class ProductInformation
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductInformation"/> class.
        /// </summary>
        public ProductInformation(string identifier, IEnumerable<ReleaseInformation> releases, int limit, int offset)
        {
            this.Identifier = identifier;
            this.Releases = releases.Skip(offset).Take(limit);
            this.TotalReleaseCount = releases.Count();
            this.NextOffset = null;
            if (this.TotalReleaseCount > offset + limit) {
                this.NextOffset = offset + limit;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="releases"></param>
        public ProductInformation(string identifier, IEnumerable<ReleaseInformation> releases)
            : this(identifier, releases, releases.Count(), 0)
        { }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// Gets the identifier of the product
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Gets the available releases of the product
        /// </summary>
        public IEnumerable<ReleaseInformation> Releases { get; }

        /// <summary>
        /// Gets the total count of releases for the product.
        /// </summary>
        public int TotalReleaseCount { get; }

        /// <summary>
        /// Gets the offset of the next release.
        /// </summary>
        public int? NextOffset { get; }
        
        #endregion
    }
}