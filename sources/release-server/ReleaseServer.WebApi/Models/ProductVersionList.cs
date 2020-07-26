//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionList.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersionList
    {
        public List<ProductVersion> Versions { get; set; }

        public ProductVersionList(List<ProductVersion> productVersions)
        {
            Versions = productVersions;
        }
    }
}