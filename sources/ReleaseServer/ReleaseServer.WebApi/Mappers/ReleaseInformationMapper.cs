//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInformationMapper.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using ReleaseServer.WebApi.Models;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReleaseInformationMapper
    {
        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<ReleaseInformation> Map(IEnumerable<DeploymentInformation> deployments)
        {
            var releases = new List<ReleaseInformation>();

            var deploymentsByVersion = GetDeploymentsMappedByVersion(deployments);

            foreach (var (version, deploymentList) in deploymentsByVersion) {
                var platforms = new List<Platform>();
                var artifacts = new List<ArtifactInformation>();
                foreach (var deployment in deploymentList) {
                    var platform = new Platform(deployment.Os, deployment.Architecture);
                    platforms.Add(platform);
                    artifacts.Add(new ArtifactInformation {
                        Platform = platform,
                        FileName = deployment.ArtifactFileName,
                        Size = deployment.ArtifactSize,
                        Sha256 = deployment.Sha256,
                        DownloadUrl = CreateDownloadUrl(deployment)
                    });
                }

                // Note: We assume here that every deployment has equivalent release notes. This will be fixed in a ReleaseArtifactService/ReleaseArtifactRepository rework.
                releases.Add(new ReleaseInformation(version, deploymentList.First().ReleaseNotes, platforms, artifacts));
            }

            return releases;
        }

        private static string CreateDownloadUrl(DeploymentInformation deployment)
        {
            // The version directory name is used, so the URL also works if it differs from the normalized version
            var segments = new[] { deployment.Identifier, deployment.Os, deployment.Architecture, deployment.VersionDirectoryName ?? deployment.Version.ToString() };
            return "/artifacts/" + string.Join("/", segments.Select(System.Uri.EscapeDataString));
        }

        private static Dictionary<ProductVersion, List<DeploymentInformation>> GetDeploymentsMappedByVersion(IEnumerable<DeploymentInformation> deployments)
        {
            var result = new Dictionary<ProductVersion, List<DeploymentInformation>>();

            foreach (var deployment in deployments) {
                List<DeploymentInformation> deploymentList;
                var version = deployment.Version;
                
                if (!result.ContainsKey(version)) {
                    deploymentList = new List<DeploymentInformation>();
                    result.Add(deployment.Version, deploymentList);
                }
                else {
                    deploymentList = result[version];
                }

                deploymentList.Add(deployment);
            }

            return result;
        }
    }
}