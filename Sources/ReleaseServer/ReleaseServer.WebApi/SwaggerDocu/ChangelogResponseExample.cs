using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ChangelogResponseExample : IExamplesProvider<ChangelogResponseModel>
    {
        public ChangelogResponseModel GetExamples()
        {
            return new ChangelogResponseModel
            {
                Changelog = "Release 1.0.0 - This is an example."
            };
        }
    }
}