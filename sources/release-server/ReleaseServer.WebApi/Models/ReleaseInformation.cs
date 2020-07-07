//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInformation.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseInformation
    {
        
        //Will be extended depending on the further requirements
        public ReleaseNotes ReleaseNotes { get; set;}

        public DateTime ReleaseDate { get; set;}
    }
}