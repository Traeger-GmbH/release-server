using NLog.Targets.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationList
    {
        public List<ProductInformation> ProductInformation;

        public ProductInformationList(List<ProductInformation> productInfoList)
            : this(productInfoList, 0, 0)
        { }

        public ProductInformationList(List<ProductInformation> productInfoList, int limit, int offset)
        {
            productInfoList.Skip(offset).Take(limit);
        }
    }
}