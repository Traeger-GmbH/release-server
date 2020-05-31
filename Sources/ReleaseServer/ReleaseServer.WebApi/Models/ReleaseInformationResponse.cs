using System;
using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseInformationResponse
    {
        public Dictionary<string, List<ChangeSet>> ReleaseNotes { get; set;}

        public DateTime ReleaseDate { get; set;}
    }
}