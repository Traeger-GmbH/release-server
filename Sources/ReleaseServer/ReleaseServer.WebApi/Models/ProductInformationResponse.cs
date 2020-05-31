using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationListResponse
    {
        public List<ProductInformationResponse> ProductInformation { get; set; }
    }
    
    public class ProductInformationResponse
    {
        public string Identifier { get; set; }
        
        public string Version { get; set;  }

        public string Os { get; set; }
        
        public string Architecture { get; set; }
        
        public Dictionary<string, List<ChangeSet>> ReleaseNotes { get; set; }
    }
}