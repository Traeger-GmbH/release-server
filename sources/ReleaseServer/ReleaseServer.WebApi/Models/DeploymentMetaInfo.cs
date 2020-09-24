//--------------------------------------------------------------------------------------------------
// <copyright file="DeploymentMetaInfo.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the meta information of the uploaded artifact. 
    /// </summary>
    public class DeploymentMetaInfo : JsonSerializable<DeploymentMetaInfo>
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
        
        /// <summary>
        /// Gets or sets the release date (required).
        /// </summary>
        /// <value>The release date of the artifact (is a required field).</value>
        [JsonRequired]
        public DateTime ReleaseDate { get; set;}
    }
}