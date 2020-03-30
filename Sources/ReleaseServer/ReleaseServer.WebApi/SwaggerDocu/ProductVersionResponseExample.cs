using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ProductVersionResponseExample : IExamplesProvider<ProductVersionResponseModel>
    {
        public ProductVersionResponseModel GetExamples()
        {
            return new ProductVersionResponseModel
            {
                Version = "1.5"
            };
        }
    }
}