using System.IO;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Common
{
    public class JsonSerializable<T>
    {
        public static T FromJsonFile(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }
    }
}