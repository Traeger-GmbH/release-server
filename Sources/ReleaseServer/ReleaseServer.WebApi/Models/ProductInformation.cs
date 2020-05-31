namespace ReleaseServer.WebApi.Models
{
    public class ProductInformation
    {
        public string ProductIdentifier { get; set; }
        
        public ProductVersion Version { get; set; }
        
        public string Os { get; set; }
        
        public string HwArchitecture { get; set; }
    }
}