//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionResponseExample.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    internal class ProductVersionResponseExample : IExamplesProvider<ProductVersionResponse>
    {
        public ProductVersionResponse GetExamples()
        {
            return new ProductVersionResponse(new ProductVersion("1.1"));
        }
        
    }
}