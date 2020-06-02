using System.Collections.Generic;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class PlatformsMapper
    {
        public static PlatformsResponse ToPlatformsResponse(this List<string> platforms)
        {
            return new PlatformsResponse
            {
                Platforms = platforms
            };
        }
    }
}