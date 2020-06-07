using System.IO;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Config;

namespace ReleaseServer.WebApi.Common
{
    public class JsonSerializable<T>
    {
        public static T FromJsonFile(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), JsonConfiguration.Settings);
        }
    }
}