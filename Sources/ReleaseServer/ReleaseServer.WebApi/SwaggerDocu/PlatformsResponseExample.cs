using System.Collections.Generic;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class PlatformsResponseExample : IExamplesProvider<PlatformsResponseModel>
    {
        public PlatformsResponseModel GetExamples()
        {
            return new PlatformsResponseModel
            {
                Platforms =  new List<string>()
                {
                    "debian-amd64",
                    "debian-arm64"
                }
            };
        }
    }
}