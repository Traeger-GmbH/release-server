namespace ReleaseServer.WebApi.Config
{
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using JsonConverters;

    public static class JsonConfiguration
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