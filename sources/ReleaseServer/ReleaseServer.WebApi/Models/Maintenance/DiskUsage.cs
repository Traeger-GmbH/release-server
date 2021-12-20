//--------------------------------------------------------------------------------------------------
// <copyright file="DiskUsage.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides information about the disk usage in the server.
    /// </summary>
    public class DiskUsage
    {
        /// <summary>
        /// Gets or sets the total disk size of the server.
        /// </summary>
        /// <value>The total disk size of the server in MBytes.</value>
        public int TotalSize { get; set; }

        /// <summary>
        /// Gets or sets the space used by the server.
        /// </summary>
        /// <value>The disk space used by the server in MBytes.</value>
        public int UsedDiskSpace { get; set; }

        /// <summary>
        /// Gets or sets the available disk space on the server.
        /// </summary>
        /// <value>The available disk space on the server in MBytes.</value>
        public int AvailableFreeSpace { get; set; }
    }
}