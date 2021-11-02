//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactMapper.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System.IO;
using System.IO.Compression;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// A mapper class that enables operations for <see cref="ReleaseArtifact"/> objects.
    /// </summary>
    public static class ReleaseArtifactMapper
    {
        #region ---------- Private constants ----------

        private const string DeploymentMetaInformationFileName = "deployment.json";

        #endregion

        #region ---------- Public static methods ----------

        /// <summary>
        /// Converts meta information and a <see cref="ZipArchive"/> to a <see cref="ReleaseArtifact"/>.
        /// </summary>
        /// <param name="product">The product name of the release artifact.</param>
        /// <param name="os">The operating system of the release artifact.</param>
        /// <param name="architecture">The HW architecture of the release artifact.</param>
        /// <param name="version">The version of the release artifact</param>
        /// <param name="payload">The <see cref="ZipArchive"/> that contains the release artifact.</param>
        /// <returns>The created <see cref="ReleaseArtifact"/>.</returns>
        public static ReleaseArtifact ConvertToReleaseArtifact(string product, string os, string architecture,
            string version, ZipArchive payload)
        {
            var deploymentMetaInformation = DeploymentMetaInformation.FromJsonFile(payload.GetEntry(DeploymentMetaInformationFileName));
            var releaseNotes = ReleaseNotes.FromJsonFile(payload.GetEntry(deploymentMetaInformation.ReleaseNotesFileName));

            return new ReleaseArtifact {
                DeploymentInformation = new DeploymentInformation {
                    Identifier = product,
                    Os = os,
                    Architecture = architecture,
                    Version = new ProductVersion(version),
                    ReleaseNotes = releaseNotes
                },
                DeploymentMetaInformation = deploymentMetaInformation,
                Content = payload.GetEntry(deploymentMetaInformation.ArtifactFileName).Open()
            };
        }

        /// <summary>
        /// Converts meta information and a <see cref="Stream"/> to a <see cref="ReleaseArtifact"/>.
        /// </summary>
        /// <returns>The created <see cref="ReleaseArtifact"/>.</returns>
        public static ReleaseArtifact ConvertToReleaseArtifact(DeploymentInformation deploymentInformation, DeploymentMetaInformation metaInformation, Stream content)
        {
            return new ReleaseArtifact {
                DeploymentInformation = deploymentInformation,
                DeploymentMetaInformation = metaInformation,
                Content = content
            };
        }

        #endregion
    }
}