namespace ReleaseServer.WebApi.Models
{
    public class ProductInformation
    {
        public string Identifier { get; set; }
        
        public ProductVersion Version { get; set; }
        
        public string Os { get; set; }
        
        public string Architecture { get; set; }
        
        public ReleaseNotes ReleaseNotes { get; set; }
    }
}