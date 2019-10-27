using System;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public class ProductInformationMapper
    {
        public static ProductInformationModel PathToProductInfo(string path)
        {
            var infos = path.Split('/', '\\');

            //If the directory has a depth of 5 (actual our standard artifact)
            if (infos.Length == 5)
            {
                return new ProductInformationModel
                {
                    ProductIdentifier = infos[1],
                    Os = infos[2],
                    HwArchitecture = infos[3],
                    Version = infos[4]
                };
            }
            return null;
        }
    }
}

