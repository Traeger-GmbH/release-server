using System.IO;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Config;

namespace ReleaseServer.WebApi.Mappers
{
    public static class DeploymentMetaInfoMapper
    {
        public static DeploymentMetaInfo ParseDeploymentMetaInfo(string fileName)
        {
            return JsonConvert.DeserializeObject<DeploymentMetaInfo>(File.ReadAllText(fileName));
        }
    }
}