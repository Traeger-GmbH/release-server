//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionListResponseExample.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    internal class ProductVersionListExample : IExamplesProvider<ProductVersionList>
    {
        public ProductVersionList GetExamples()
        {
            return new ProductVersionList(new List<ProductVersion>(new[]
            {
                new ProductVersion("1.1"),
                new ProductVersion("1.0")
            }));
        }
    }
}