using System;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseInformationModel
    {
        
        //Will be extended depending on the further requirements
        public string ReleaseNotes { get; set;}

        public DateTime ReleaseDate { get; set;}
    }
}