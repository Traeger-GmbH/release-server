using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

public class ProductInformationListResponseExample : IExamplesProvider<ProductInformationListResponseModel>
{
    public ProductInformationListResponseModel GetExamples()
    {
        return new ProductInformationListResponseModel
        {
            ProductInformation = new List<ProductInformationResponseModel>
            {
                new ProductInformationResponseModel
                {
                    Identifier = "softwareX",
                    Version = "1.0",
                    Os = "debian",
                    Architecture = "amd64"
                },
                new ProductInformationResponseModel
                {
                    Identifier = "softwareX",
                    Version = "1.1-beta",
                    Os = "debian",
                    Architecture = "amd64"
                }
            }
        };
    }
}