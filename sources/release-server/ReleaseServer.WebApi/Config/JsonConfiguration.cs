//--------------------------------------------------------------------------------------------------
// <copyright file="JsonConfiguration.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi
{
    internal static class JsonConfiguration
    {
        public static void Configure(MvcNewtonsoftJsonOptions options)
        {
            Configure(options.SerializerSettings);
        }

        public static void Configure(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new ProductVersionConverter());
        }

        public static JsonSerializerSettings Settings
        {
            get
            {
                var settings = new JsonSerializerSettings();
                Configure(settings);
                return settings;
            }
        }
    }
}