using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersionList
    {
        public List<ProductVersion> Versions;

        public ProductVersionList(List<ProductVersion> productVersions)
        {
            Versions = productVersions;
        }
    }
}