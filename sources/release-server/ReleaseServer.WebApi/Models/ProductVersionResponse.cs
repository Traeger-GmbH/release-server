//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionResponse.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersionResponse
    {
        //TODO: Handle with a custom json converter!
        public string Version { get; set; }

        public ProductVersionResponse(ProductVersion productVersion)
        {
            Version = productVersion.ToString();
        }
    }
}