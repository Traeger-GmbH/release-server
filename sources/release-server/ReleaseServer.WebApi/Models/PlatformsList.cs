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