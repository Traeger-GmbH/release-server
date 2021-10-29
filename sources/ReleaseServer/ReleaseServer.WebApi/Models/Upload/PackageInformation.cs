//--------------------------------------------------------------------------------------------------
// <copyright file="PackageInformation.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

using Newtonsoft.Json;
using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the meta information of the uploaded package (described by package.json).
    /// </summary>
    public class PackageInformation : JsonSerializable<PackageInformation>
    {
        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or sets the identifier of this package.
        /// </summary>
        [JsonRequired]
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the release notes of this package.
        /// </summary>
        [JsonRequired]
        public ReleaseNotes ReleaseNotes { get; set; }

        /// <summary>
        /// Gets or sets the mapping of platform specific deployments and platforms of this package.
        /// </summary>
        [JsonRequired]
        public Dictionary<string, string> Deployments { get; set; }

        #endregion
    }
}
