//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseNotes.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the the release notes with all the changes.
    /// </summary>
    public class ReleaseNotes : JsonSerializable<ReleaseNotes>
    {
        
        /// <summary>
        /// Gets or sets the code / product changes with this release (required).
        /// </summary>
        [JsonRequired]
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
}