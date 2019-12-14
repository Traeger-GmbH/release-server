using NSubstitute.Core;

namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationResponseModel
    {
        public string Identifier { get; set; }
        
        public string Version { get; set;  }

        public string Os { get; set; }
        
        public string Architecture { get; set; }
    }
}