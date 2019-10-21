using System;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public class ProductInformationMapper
    {
        public static ProductInformationModel PathToProductInfo(string path)
        {
            var infos = path.Split('/', '\\');

            //If the directory has a depth of 4 (actual our standard artifact)
            if (infos.Length == 5)
            {
                return new ProductInformationModel
                {
                    ProductIdentifier = infos[1],
                    Version = infos[2],
                    Os = infos[3],
                    HwArchitecture = infos[4]
                };
            }
            return null;
        }
    }
}

