using System.Collections.Generic;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class PlatformsMapper
    {
        public static PlatformsResponseModel ToPlatformsResponse(this List<string> platforms)
        {
            return new PlatformsResponseModel
            {
                Platforms = platforms
            };
        }
    }
}