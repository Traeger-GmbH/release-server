using System.Collections.Generic;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ProductInformationResponseMapper
    {
        public static ProductInformationResponse ToProductInfoResponse(this ProductInformation productInfo)
        {
            return new ProductInformationResponse
            {
                Identifier = productInfo.ProductIdentifier,
                Os = productInfo.Os,
                Version = productInfo.Version.ToString(),
                Architecture = productInfo.HwArchitecture
            };
        }

        public static ProductInformationListResponse ToProductInfoListResponse(this List<ProductInformation> productInfoList)
        {
            var retVal = new List<ProductInformationResponse>();

            foreach (var productInfo in productInfoList)
            {
                retVal.Add(productInfo.ToProductInfoResponse());
            }
            
            return new ProductInformationListResponse
            {
                ProductInformation = retVal
            };
        }
    }
}

