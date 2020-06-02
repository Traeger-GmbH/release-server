using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ProductVersionResponseExample : IExamplesProvider<ProductVersionResponse>
    {
        public ProductVersionResponse GetExamples()
        {
            return new ProductVersionResponse
            {
                Version = "1.5"
            };
        }
    }
}