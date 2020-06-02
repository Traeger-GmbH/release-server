using System.IO;
using System.Text;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class DeploymentMetaInfoMapper
    {
        public static DeploymentMetaInfo ParseDeploymentMetaInfo(string fileName)
        {
            return JsonConvert.DeserializeObject<DeploymentMetaInfo>(File.ReadAllText(fileName));
        }
        
        public static DeploymentMetaInfo ParseDeploymentMetaInfo(byte[] byteContent)
        {
            return JsonConvert.DeserializeObject<DeploymentMetaInfo>(Encoding.UTF8.GetString(byteContent));
        }
    }
}