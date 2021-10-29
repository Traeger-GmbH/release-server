//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseNotes.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Timo Walter</author>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the release notes of a release.
    /// </summary>
    public class ReleaseNotes : JsonSerializable<ReleaseNotes>
    {
        /// <summary>
        /// Gets or sets the version of this release.
        /// </summary>
        [JsonRequired]
        public ProductVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the release date (required).
        /// </summary>
        /// <value>The release date of the artifact (is a required field).</value>
        [JsonRequired]
        public DateTime Date { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsPreviewRelease { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public bool IsSecurityPatch { get; set; } = false;

        /// <summary>
        /// Gets or sets the platforms this release is available for.
        /// </summary>
        [JsonRequired]
        public List<string> Platforms { get; set; }

        /// <summary>
        /// Gets or sets the code / product changes with this release (required).
        /// </summary>
        /// <value>The changes of the code / product within a release. This is required field.</value>
        [JsonRequired]
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set; }
    }
}
