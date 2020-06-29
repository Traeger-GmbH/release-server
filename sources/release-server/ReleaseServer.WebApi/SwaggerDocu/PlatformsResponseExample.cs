using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class PlatformsResponseExample : IExamplesProvider<PlatformsList>
    {
        public PlatformsList GetExamples()
        {
            return new PlatformsList(new List<string>(new[]{"debian-amd64", "debian-arm64"}));
        }
    }
}