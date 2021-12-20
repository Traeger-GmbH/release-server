//--------------------------------------------------------------------------------------------------
// <copyright file="Statistics.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides statistical information about the server.
    /// </summary>
    public class Statistics
    {
        /// <summary>
        /// Gets or sets the disk usage of the server.
        /// </summary>
        /// <value>The disk usage of the server.</value>
        public DiskUsage Disk { get; set; }
        
        /// <summary>
        /// Gets or sets the number of products.
        /// </summary>
        /// <value>Specifies the number of products stored in the server.</value>
        public int NumberOfProducts { get; set; }

        /// <summary>
        /// Gets or sets the number of artifacts.
        /// </summary>
        /// <value>Specifies the number of artifacts stored in the server.</value>
        public int NumberOfArtifacts { get; set; }
    }
}