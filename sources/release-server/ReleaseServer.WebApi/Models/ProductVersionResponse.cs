//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionResponse.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Defines the structure of the product version response body.
    /// </summary>
    //TODO: Handle with a custom json converter!
    public class ProductVersionResponse
    {
        /// <summary>
        /// Gets the product version as string.
        /// </summary>
        /// <value>The product version.</value>
        public string Version { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVersionResponse"/> class.
        /// </summary>
        /// <param name="productVersion">The product version.</param>
        public ProductVersionResponse(ProductVersion productVersion)
        {
            Version = productVersion.ToString();
        }
    }
}