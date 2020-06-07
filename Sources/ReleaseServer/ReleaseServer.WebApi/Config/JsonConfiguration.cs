using Newtonsoft.Json;
using ReleaseServer.WebApi.JsonConverters;

namespace ReleaseServer.WebApi.Config
{
    public static class JsonConfiguration
    {
        public static JsonSerializerSettings Settings
        {
            get
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new ProductVersionConverter());
                return settings;
            }
        }
    }
}