using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class PlatformsList
    {
        //Will be extended depending on the further requirements
        public List<string> Platforms { get; set; }

        public PlatformsList(List<string> platforms)
        {
            Platforms = platforms;
        }
    }
}