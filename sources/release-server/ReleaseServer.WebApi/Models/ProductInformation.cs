//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformation.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformation
    {
        public string Identifier { get; set; }
        
        public ProductVersion Version { get; set; }
        
        public string Os { get; set; }
        
        public string Architecture { get; set; }
        
        public ReleaseNotes ReleaseNotes { get; set; }
    }
}