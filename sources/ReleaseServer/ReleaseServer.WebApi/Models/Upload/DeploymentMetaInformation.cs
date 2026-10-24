//--------------------------------------------------------------------------------------------------
// <copyright file="DeploymentMetaInfo.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the meta information of the uploaded artifact. 
    /// </summary>
    public class DeploymentMetaInformation : JsonSerializable<DeploymentMetaInformation>
    {
        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or sets the filename of the release notes (required).
        /// </summary>
        /// <value>The filename of the release notes (is a required field).</value>
        [JsonRequired]
        public string ReleaseNotesFileName { get; set; }
        
        /// <summary>
        /// Gets or sets the artifact filename (required).
        /// </summary>
        /// <value>The name of the artifact file (is a required field).</value>
        [JsonRequired]
        public string ArtifactFileName { get; set; }

        /// <summary>
        /// Gets or sets the SHA-256 checksum of the artifact file (lower case hex). Written since version 1.1;
        /// older data gets it added by the server. Previous versions ignore it.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Sha256 { get; set; }

        #endregion
    }
}