using System;
using System.Collections.Generic;
using System.Globalization;
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
                //TODO: Fill the object with examples!
                ReleaseNotes = new ReleaseNotesModel(),
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
    }
}