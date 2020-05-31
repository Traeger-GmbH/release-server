using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersionResponse
    {
        //Will be extended depending on the further requirements
        public string Version { get; set; }
    }
    
    public class ProductVersionListResponse
    {
        //Will be extended depending on the further requirements
        public List<string> Versions { get; set; }
    }
}