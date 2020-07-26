//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformationList.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationList
    {
        public List<ProductInformation> ProductInformation { get; set; }

        public ProductInformationList(List<ProductInformation> productInfoList)
        {
            ProductInformation = productInfoList;
        }
    }
}