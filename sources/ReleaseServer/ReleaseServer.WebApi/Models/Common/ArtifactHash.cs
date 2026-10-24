//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactHash.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// A calculated checksum of an artifact file together with the file state it was calculated for.
    /// </summary>
    public class ArtifactHash
    {
        /// <summary>Gets or sets the full path of the release directory.</summary>
        public string ReleaseDirectory { get; set; }

        /// <summary>Gets or sets the artifact file name from deployment.json.</summary>
        public string ArtifactFileName { get; set; }

        /// <summary>Gets or sets the SHA-256 checksum (lower case hex).</summary>
        public string Sha256 { get; set; }

        /// <summary>Gets or sets the file length the checksum was calculated for.</summary>
        public long Length { get; set; }

        /// <summary>Gets or sets the last write time the checksum was calculated for.</summary>
        public DateTime LastWriteTimeUtc { get; set; }
    }
}
