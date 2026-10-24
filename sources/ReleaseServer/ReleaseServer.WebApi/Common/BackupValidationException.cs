//--------------------------------------------------------------------------------------------------
// <copyright file="BackupValidationException.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Thrown when an uploaded backup cannot be restored. The existing data stays unchanged.
    /// </summary>
    public class BackupValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackupValidationException"/> class.
        /// </summary>
        /// <param name="errors">The problems found in the backup.</param>
        public BackupValidationException(IEnumerable<string> errors)
            : base("The backup cannot be restored: " + string.Join(" ", errors))
        {
            Errors = errors.ToList();
        }

        /// <summary>
        /// Gets the problems found in the backup.
        /// </summary>
        public IReadOnlyList<string> Errors { get; }
    }
}
