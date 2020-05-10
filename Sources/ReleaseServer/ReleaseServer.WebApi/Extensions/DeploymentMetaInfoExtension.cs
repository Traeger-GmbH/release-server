using System;
using ReleaseServer.WebApi.Config;

namespace ReleaseServer.WebApi.Extensions
{
    public static class DeploymentMetaInfoExtension
    {
        public static bool IsValid(this DeploymentMetaInfoModel deploymentMetaInfoModel)
        {
            if (deploymentMetaInfoModel.ReleaseDate == new DateTime() || deploymentMetaInfoModel.ArtifactFileName == null ||
                deploymentMetaInfoModel.ChangelogFileName == null)
            {
                return false;
            }

            return true;
        }
    }
}