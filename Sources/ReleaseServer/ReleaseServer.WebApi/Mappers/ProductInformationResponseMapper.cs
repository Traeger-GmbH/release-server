using System.Collections.Generic;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ProductInformationResponseMapper
    {
        public static ProductInformationResponseModel ToProductInfoResponse(this ProductInformation productInfo)
        {
            return new ProductInformationResponseModel
            {
                Identifier = productInfo.ProductIdentifier,
                Os = productInfo.Os,
                Version = productInfo.Version.ToString(),
                Architecture = productInfo.HwArchitecture,
                ReleaseNotes = productInfo.ReleaseNotes.GetReleaseNotesResponse()
            };
        }

        public static ProductInformationListResponse ToProductInfoListResponse(this List<ProductInformation> productInfoList)
        {
            var retVal = new List<ProductInformationResponseModel>();

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

