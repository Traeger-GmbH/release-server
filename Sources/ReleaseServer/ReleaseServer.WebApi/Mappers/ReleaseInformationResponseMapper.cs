using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseInformationResponseMapper
    {
        public static ReleaseInformationResponseModel ToReleaseInformationResponse(this ReleaseInformationModel releaseInfo)
        {
            return new ReleaseInformationResponseModel
            {
                ReleaseNotes = releaseInfo.ReleaseNotes.GetReleaseNotesResponse(),
                ReleaseDate = releaseInfo.ReleaseDate
            };
        }
    }
}