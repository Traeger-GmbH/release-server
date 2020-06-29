using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ProductVersionListExample : IExamplesProvider<ProductVersionList>
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