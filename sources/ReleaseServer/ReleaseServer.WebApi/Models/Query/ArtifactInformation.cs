//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// The downloadable file of a release for one platform.
    /// </summary>
    public class ArtifactInformation
    {
        /// <summary>
        /// Gets or sets the platform (operating system and architecture, e.g. "windows/x64").
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Gets or sets the file name of the download.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the size of the file in bytes.
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// Gets or sets the SHA-256 checksum of the file as lower case hex string. It is null for existing
        /// files until the server has calculated it (shortly after the start).
        /// </summary>
        public string Sha256 { get; set; }

        /// <summary>
        /// Gets or sets the relative URL to download the file.
        /// </summary>
        public string DownloadUrl { get; set; }
    }
}
