using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ReleaseInformationResponseMapper
    {
        public static ReleaseInformationResponse ToReleaseInformationResponse(this ReleaseInformation releaseInfo)
        {
            return new ReleaseInformationResponse
            {
                ReleaseNotes = releaseInfo.ReleaseNotes,
                ReleaseDate = releaseInfo.ReleaseDate
            };
        }
    }
}