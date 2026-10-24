//--------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Extensions for operations on <see cref="IEnumerable{T}"/>.
    /// </summary>
    internal static class EnumerableExtensions
    {
        /// <summary>
        /// Determines whether the sequence is null or does not contain any elements.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}
