namespace ReleaseServer.WebApi.Models
{
    public class ProductInformationModel
    {
        public string ProductIdentifier { get; set; }
        
        public string Version { get; set; }
        
        public string Os { get; set; }
        
        public string HwArchitecture { get; set; }
    }
}