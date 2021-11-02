//--------------------------------------------------------------------------------------------------
// <copyright file="JsonConfiguration.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace ReleaseServer.WebApi
{
    internal static class JsonConfiguration
    {
        #region ---------- Public static properties ----------
        
        public static JsonSerializerSettings Settings
        {
            get
            {
                var settings = new JsonSerializerSettings();
                Configure(settings);
                return settings;
            }
        }

        #endregion

        #region ---------- Private static properties ----------

        private static NamingStrategy NamingStrategy
        {
            get
            {
                var strategy = new CamelCaseNamingStrategy();
                return strategy;
            }
        }
        
        #endregion

        #region ---------- Public static methods ----------

        public static void Configure(MvcNewtonsoftJsonOptions options)
        {
            Configure(options.SerializerSettings);
        }

        public static void Configure(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new ProductVersionConverter());
            settings.Converters.Add(new PlatformConverter());
            settings.Converters.Add(new StringEnumConverter(namingStrategy: NamingStrategy, allowIntegerValues: false));
        }
        
        #endregion
    }
}