//--------------------------------------------------------------------------------------------------
// <copyright file="BackupInformation.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information, where the backup is stored and what's the name of the backup file name.
    /// </summary>
    public class BackupInformation
    {
        /// <summary>
        /// Gets or sets the full path of the directory, where the backup is stored.
        /// </summary>
        /// <value>The full path of the backup directory.</value>
        public string FullPath { get; set; }
        
        /// <summary>
        /// Gets or sets the filename of the backup.
        /// </summary>
        /// <value>Specifies the file name of the created backup file.</value>
        public string FileName { get; set; }
    }
}