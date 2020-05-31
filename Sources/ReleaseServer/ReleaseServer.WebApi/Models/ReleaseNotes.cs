using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseNotes
    {
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
}