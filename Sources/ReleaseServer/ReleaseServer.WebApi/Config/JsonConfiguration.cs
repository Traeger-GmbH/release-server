namespace ReleaseServer.WebApi.Config
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;
    using ReleaseServer.WebApi.JsonConverters;

    public static class JsonConfiguration
    {
        public static void Configure(MvcNewtonsoftJsonOptions options)
        {
            Configure(options.SerializerSettings);
        }

        public static void Configure(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new ProductVersionConverter());
            settings.Converters.Add(new StringEnumConverter(namingStrategy: NamingStrategy, allowIntegerValues: false));
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

        private static NamingStrategy NamingStrategy
        {
            get
            {
                var strategy = new CamelCaseNamingStrategy();
                return strategy;
            }
        }
    }
}