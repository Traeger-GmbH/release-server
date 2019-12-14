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
                Architecture = productInfo.HwArchitecture
            };
        }
    }
}

