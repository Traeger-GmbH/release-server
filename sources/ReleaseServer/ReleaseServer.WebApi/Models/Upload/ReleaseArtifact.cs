//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifact.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the content of a single artifact.
    /// </summary>
    public class ReleaseArtifact : IDisposable
    {
        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or sets the product information of the artifact.
        /// </summary>
        /// <value>The product information of the artifact in form of an <see cref="DeploymentInformation"/> object.</value>
        public DeploymentInformation DeploymentInformation { get; set; }

        /// <summary>
        /// Gets or sets the meta information of the artifact.
        /// </summary>
        /// <value>The meta information of the artifact in form of an <see cref="DeploymentMetaInformation"/> object.</value>
        public DeploymentMetaInformation DeploymentMetaInformation { get; set; }

        /// <summary>
        /// Gets or sets the content of the artifact in form of a <see cref="Stream"/>.
        /// </summary>
        /// <value>The content of the release artifact in form of a <see cref="Stream"/>.</value>
        public Stream Content { get; set; }

        #endregion

        #region ---------- Public methods ----------

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (this.Content != null) {
                this.Content.Dispose();
            }
        }

        #endregion
    }
}