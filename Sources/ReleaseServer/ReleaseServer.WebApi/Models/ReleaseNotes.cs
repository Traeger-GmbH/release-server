using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseNotes
    {
        [JsonRequired]
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
}