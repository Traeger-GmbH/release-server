using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseNotesMapper
    {
        public static ReleaseNotes ParseReleaseNotes(string fileName)
        {
            var parsedReleaseNotes = JsonConvert.DeserializeObject<Dictionary<CultureInfo, List<ChangeSet>>>(File.ReadAllText(fileName));

            return new ReleaseNotes
            {
                Changes = parsedReleaseNotes
            };
        }
    }
}