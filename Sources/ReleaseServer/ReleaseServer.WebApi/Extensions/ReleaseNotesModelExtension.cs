using System.Collections.Generic;
using System.Linq;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Extensions
{
    public static class ReleaseNotesModelExtension
    {
        public static Dictionary<string, List<ChangeSet>> GetReleaseNotesResponse(this ReleaseNotes releaseNotes)
        {
            if (releaseNotes == null)
                return new Dictionary<string, List<ChangeSet>>();

            var responseReleaseNotes = releaseNotes.ReleaseNotesSet.ToDictionary(
                releaseNotesSubSet => releaseNotesSubSet.Key.ToString(),
                releaseNotesSubSet => releaseNotesSubSet.Value);

            return responseReleaseNotes;
        }
    }
}