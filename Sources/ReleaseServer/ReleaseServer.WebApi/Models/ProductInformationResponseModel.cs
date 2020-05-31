using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationListResponse
    {
        public List<ProductInformationResponseModel> ProductInformation { get; set; }
    }
    
    public class ProductInformationResponseModel
    {
        public string Identifier { get; set; }
        
        public string Version { get; set;  }

        public string Os { get; set; }
        
        public string Architecture { get; set; }
        
        public Dictionary<string, List<ChangeSet>> ReleaseNotes { get; set; }
    }
}