//--------------------------------------------------------------------------------------------------
// <copyright file="PlatformsList.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information of the available platforms for a specific artifact.
    /// </summary>
    public class PlatformsList
    {
        /// <summary>
        /// Gets the available platforms for the corresponding artifact.
        /// </summary>
        /// <value>The available platforms for the corresponding artifact (e.g. ubuntu-amd64, debian-arm64).</value>
        public List<string> Platforms { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlatformsList"/> class.
        /// </summary>
        /// <param name="platforms">The list of platforms.</param>
        public PlatformsList(List<string> platforms)
        {
            Platforms = platforms;
        }
    }
}