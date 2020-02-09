using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ChangelogResponseModelMapper
    {
        public static ChangelogResponseModel toChangelogResponse(this string changelog)
        {
            return new ChangelogResponseModel
            {
                Changelog = changelog
            };
        }
    }
}