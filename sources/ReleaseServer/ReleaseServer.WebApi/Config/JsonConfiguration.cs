//--------------------------------------------------------------------------------------------------
// <copyright file="JsonConfiguration.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

using ReleaseServer.WebApi.Models;

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

        #region ---------- Private static fields ----------

        private static readonly object PrepareLock = new object();
        private static bool contractsPrepared;

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

        /// <summary>
        /// Deserializes every model type once. Newtonsoft.Json (13.0.3) initializes the check of required
        /// properties lazily and not thread-safe: if the first deserializations of a type run concurrently,
        /// one of them can skip the [JsonRequired] check. Calling this method once before concurrent use
        /// prevents that.
        /// </summary>
        public static void PrepareContracts()
        {
            lock (PrepareLock)
            {
                if (contractsPrepared)
                    return;

                foreach (var type in new[] { typeof(DeploymentMetaInformation), typeof(ReleaseNotes), typeof(PackageInformation), typeof(ChangeSet) })
                {
                    foreach (var serializer in new[] { new JsonSerializer(), JsonSerializer.Create(Settings) })
                    {
                        try
                        {
                            serializer.Deserialize(new StringReader("{}"), type);
                        }
                        catch (JsonSerializationException)
                        {
                            // Expected: required properties are missing
                        }
                    }
                }

                contractsPrepared = true;
            }
        }

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