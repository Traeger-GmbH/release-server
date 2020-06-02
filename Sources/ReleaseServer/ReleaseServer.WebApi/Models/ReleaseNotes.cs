using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Common;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseNotes : JsonSerializable<ReleaseNotes>
    {
        [JsonRequired]
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
}