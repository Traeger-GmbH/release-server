using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ProductVersionResponseExample : IExamplesProvider<ProductVersionListResponseModel>
    {
        public ProductVersionListResponseModel GetExamples()
        {
            return new ProductVersionListResponseModel
            {
                Versions = new List<string>()
                {
                    "1.1",
                    "1.0"
                }
            };
        }
    }
}