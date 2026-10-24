//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides a list of product information for several artifacts.
    /// </summary>
    public class ReleaseInformation : ReleaseNotes
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseInformation"/> class.
        /// </summary>
        public ReleaseInformation(ProductVersion version, ReleaseNotes releaseNotes, IEnumerable<Platform> platforms)
            : this(version, releaseNotes, platforms, new List<ArtifactInformation>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseInformation"/> class with the downloadable files.
        /// </summary>
        public ReleaseInformation(ProductVersion version, ReleaseNotes releaseNotes, IEnumerable<Platform> platforms,
            IEnumerable<ArtifactInformation> artifacts)
        {
            this.ReleaseDate = releaseNotes.ReleaseDate;
            this.Changes = releaseNotes.Changes;
            this.IsPreviewRelease = releaseNotes.IsPreviewRelease;
            this.IsSecurityPatch = releaseNotes.IsSecurityPatch;
            this.Version = version;
            this.Platforms = platforms;
            this.Artifacts = artifacts;
        }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or sets the version of this release.
        /// </summary>
        [JsonRequired]
        public ProductVersion Version { get; set; }

        /// <summary>
        /// Gets or sets the platforms this release is available for.
        /// </summary>
        [JsonRequired]
        public IEnumerable<Platform> Platforms { get; set; }

        /// <summary>
        /// Gets or sets the downloadable files of the release (one per platform, same order as Platforms).
        /// Added in version 1.1; it is serialized last, so the existing properties keep their order.
        /// </summary>
        [JsonProperty(Order = 1)]
        public IEnumerable<ArtifactInformation> Artifacts { get; set; }

        #endregion
    }
}