//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the structure of the release information
    /// </summary>
    public class ReleaseInformation
    {
        
        /// <summary>
        /// Gets or sets the release notes.
        /// </summary>
        /// <value>The release notes of the artifact in form of an <see cref="ReleaseNotes"/> object.</value>
        public ReleaseNotes ReleaseNotes { get; set;}

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        /// <value>The release date of the artifact</value>
        public DateTime ReleaseDate { get; set;}
    }
}