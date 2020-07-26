//--------------------------------------------------------------------------------------------------
// <copyright file="ChangeSet.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information of the code changes of a certain release.
    /// </summary>
    public class ChangeSet
    {
        /// <summary>
        /// Gets or sets the platforms, that are compatible with the corresponding artifact / product.
        /// </summary>
        public List<string> Platforms { get; set; }
    
        /// <summary>
        /// Gets or sets the added features of the release.
        /// </summary>
        public List<string> Added { get; set; }
    
        /// <summary>
        /// Gets or sets the issues, that have been fixed
        /// </summary>
        public List<string> Fixed { get; set; }
    
        /// <summary>
        /// Gets or sets the breaking changes of the release.
        /// </summary>
        public List<string> Breaking { get; set; }
    
        /// <summary>
        /// Gets or sets the deprecated components.
        /// </summary>
        public List<string> Deprecated { get; set; }
    }
}