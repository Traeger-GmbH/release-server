using System;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Extensions
{
    public static class DeploymentMetaInfoExtension
    {
        public static bool IsValid(this DeploymentMetaInfo deploymentMetaInfo)
        {
            if (deploymentMetaInfo.ReleaseDate == new DateTime() || deploymentMetaInfo.ArtifactFileName == null ||
                deploymentMetaInfo.ReleaseNotesFileName == null)
            {
                return false;
            }

            return true;
        }
    }
}