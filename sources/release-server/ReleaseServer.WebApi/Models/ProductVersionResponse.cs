using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersionResponse
    {
        //TODO: Handle with a custom json converter!
        public string Version { get; set; }

        public ProductVersionResponse(ProductVersion productVersion)
        {
            Version = productVersion.ToString();
        }
    }
}