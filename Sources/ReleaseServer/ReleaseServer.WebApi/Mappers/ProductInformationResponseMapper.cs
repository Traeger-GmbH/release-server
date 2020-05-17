using System.Collections.Generic;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ProductInformationResponseMapper
    {
        public static ProductInformationResponseModel ToProductInfoResponse(this ProductInformationModel productInfo)
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

        public static ProductInformationListResponseModel ToProductInfoListResponse(this List<ProductInformationModel> productInfoList)
        {
            var retVal = new List<ProductInformationResponseModel>();

            foreach (var productInfo in productInfoList)
            {
                retVal.Add(productInfo.ToProductInfoResponse());
            }
            
            return new ProductInformationListResponseModel
            {
                ProductInformation = retVal
            };
        }
    }
}

