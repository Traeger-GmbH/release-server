//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseInformation.cs" company="Traeger IndustryComponents GmbH">
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
        public ReleaseNotes ReleaseNotes { get; set;}

        /// <summary>
        /// Gets or sets the release date.
        /// </summary>
        public DateTime ReleaseDate { get; set;}
    }
}