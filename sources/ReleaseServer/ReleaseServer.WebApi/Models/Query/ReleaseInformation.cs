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
        {
            this.Date = releaseNotes.Date;
            this.Changes = releaseNotes.Changes;
            this.IsPreviewRelease = releaseNotes.IsPreviewRelease;
            this.IsSecurityPatch = releaseNotes.IsSecurityPatch;
            this.Version = version;
            this.Platforms = platforms;
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

        #endregion
    }
}