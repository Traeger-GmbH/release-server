using System;
using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseInformationResponse
    {
        public ReleaseNotes ReleaseNotes { get; set;}

        public DateTime ReleaseDate { get; set;}
    }
}