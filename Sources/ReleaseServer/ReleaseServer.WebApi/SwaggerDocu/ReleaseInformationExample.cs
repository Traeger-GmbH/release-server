using System;
using ReleaseServer.WebApi.Models;
using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi.SwaggerDocu
{
    public class ReleaseInformationExample : IExamplesProvider<ReleaseInformationModel>
    {
        public ReleaseInformationModel GetExamples()
        {
            return new ReleaseInformationModel
            {
                Changelog = "Release 1.0.0 - This is an example.",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
    }
}