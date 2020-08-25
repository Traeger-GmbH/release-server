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
        /// Gets or sets the platforms, which are affected by the changes.
        /// </summary>
        /// <value>Platforms like `windows/any` or `linux/rpi`. This property should always been set.</value>
        public List<string> Platforms { get; set; }
    
        /// <summary>
        /// Gets or sets the added features of the release.
        /// </summary>
        /// <value>Contains elements, if new features were added in this release. Empty, if no features were added.</value>
        public List<string> Added { get; set; }
    
        /// <summary>
        /// Gets or sets the issues, that have been fixed
        /// </summary>
        /// <value>Contains elements, if new something got fixed in this release. Empty, if nothing got fixed.</value>
        public List<string> Fixed { get; set; }
    
        /// <summary>
        /// Gets or sets the breaking changes of the release.
        /// </summary>
        /// <value>Contains elements, if there are breaking changes in this release. Empty, if there are now breaking changes.</value>
        public List<string> Breaking { get; set; }
    
        /// <summary>
        /// Gets or sets the deprecated components.
        /// </summary>
        /// <value>Contains elements, if there are components marked as deprecated in this release.
        /// Empty, if there are deprecated components.</value>
        public List<string> Deprecated { get; set; }
    }
}