//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactDownload.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the meta file name and payload of the downloaded artifact.
    /// </summary>
    public class ArtifactDownload
    {
        /// <summary>
        /// Gets or sets the name of the downloaded artifact file.
        /// </summary>
        /// <value>The name the downloaded artifact files.</value>
        public string FileName { get; set; }
        
        /// <summary>
        /// Gets or sets the full path of the artifact file. The file is streamed from disk when downloaded.
        /// </summary>
        /// <value>The full path of the artifact file.</value>
        public string FilePath { get; set; }
    }
}