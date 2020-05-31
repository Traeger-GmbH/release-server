using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationList
    {
        public List<ProductInformation> ProductInformation;

        public ProductInformationList(List<ProductInformation> productInfoList)
        {
            ProductInformation = productInfoList;
        }
    }
}