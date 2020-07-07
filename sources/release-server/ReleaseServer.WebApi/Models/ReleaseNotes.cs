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
    public class ReleaseNotes : JsonSerializable<ReleaseNotes>
    {
        [JsonRequired]
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
}