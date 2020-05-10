using System.IO;
using System.Text;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Config;

namespace ReleaseServer.WebApi.Mappers
{
    public static class DeploymentMetaInfoMapper
    {
        public static DeploymentMetaInfoModel ParseDeploymentMetaInfo(string fileName)
        {
            return JsonConvert.DeserializeObject<DeploymentMetaInfoModel>(File.ReadAllText(fileName));
        }
        
        public static DeploymentMetaInfoModel ParseDeploymentMetaInfo(byte[] byteContent)
        {
            return JsonConvert.DeserializeObject<DeploymentMetaInfoModel>(Encoding.UTF8.GetString(byteContent));
        }
    }
}