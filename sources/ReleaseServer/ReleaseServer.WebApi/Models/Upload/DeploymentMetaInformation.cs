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
    }
}