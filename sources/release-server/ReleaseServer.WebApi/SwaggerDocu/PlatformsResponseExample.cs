//--------------------------------------------------------------------------------------------------
// <copyright file="PlatformsResponseExample.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    internal class PlatformsResponseExample : IExamplesProvider<PlatformsList>
    {
        public PlatformsList GetExamples()
        {
            return new PlatformsList(new List<string>(new[]{"debian-amd64", "debian-arm64"}));
        }
    }
}