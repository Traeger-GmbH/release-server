//--------------------------------------------------------------------------------------------------
// <copyright file="PlatformsList.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class PlatformsList
    {
        public List<string> Platforms { get; set; }

        public PlatformsList(List<string> platforms)
        {
            Platforms = platforms;
        }
    }
}