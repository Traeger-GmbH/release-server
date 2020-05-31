using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

public class ProductInformationListResponseExample : IExamplesProvider<ProductInformationListResponse>
{
    public ProductInformationListResponse GetExamples()
    {
        return new ProductInformationListResponse
        {
            ProductInformation = new List<ProductInformationResponse>
            {
                new ProductInformationResponse
                {
                    Identifier = "softwareX",
                    Version = "1.0",
                    Os = "debian",
                    Architecture = "amd64"
                },
                new ProductInformationResponse
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