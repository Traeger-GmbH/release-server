//--------------------------------------------------------------------------------------------------
// <copyright file="StorageNames.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Text.RegularExpressions;

using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Validates the names that are used as directory and file names in the artifact directory.
    /// The rules only apply to new uploads. Existing data is never rejected; reading and deleting is
    /// protected by checking that the resolved path stays inside the artifact directory.
    /// </summary>
    public static class StorageNames
    {
        /// <summary>
        /// The name of the meta information file in every artifact directory.
        /// </summary>
        public const string DeploymentMetaFileName = "deployment.json";

        /// <summary>
        /// Directories in the artifact root starting with this prefix are internal (temporary files,
        /// restore staging) and never listed as products.
        /// </summary>
        public const string InternalDirectoryPrefix = ".";

        // Product, operating system and architecture names. Derived from the production data (e.g.
        // "s5-lan-3964R-lan-manager", "Windows/X86", "windows-xp/any", "teleprof/2"): letters of both
        // cases, digits and '-'; additionally '.' and '_' are allowed. The first character must be a
        // letter or digit, which also excludes "." and "..".
        private static readonly Regex SegmentPattern = new Regex("^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$", RegexOptions.CultureInvariant);

        private static readonly char[] InvalidFileNameCharacters = { '/', '\\', ':', '*', '?', '"', '<', '>', '|' };

        /// <summary>
        /// Checks a product, operating system or architecture name.
        /// </summary>
        public static bool IsValidSegment(string value)
        {
            return value != null && SegmentPattern.IsMatch(value);
        }

        /// <summary>
        /// Checks a version. It has to be a valid <see cref="ProductVersion"/> and its directory name
        /// (the normalized version) has to be a valid segment.
        /// </summary>
        public static bool IsValidVersion(string value)
        {
            return TryParseVersion(value, out var version) && IsValidSegment(version.ToString());
        }

        /// <summary>
        /// Tries to parse a version without throwing an exception.
        /// </summary>
        public static bool TryParseVersion(string value, out ProductVersion version)
        {
            version = null;

            if (string.IsNullOrEmpty(value))
                return false;

            try
            {
                version = new ProductVersion(value);
                return true;
            }
            catch (Exception e) when (e is ArgumentException || e is FormatException || e is OverflowException)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks the name of a file in an artifact directory (artifact and release notes file). Spaces and
        /// most special characters are allowed, path separators and characters that are invalid on common
        /// file systems are not.
        /// </summary>
        public static bool IsValidFileName(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                   && value.Length <= 255
                   && value != "."
                   && value != ".."
                   && value.IndexOfAny(InvalidFileNameCharacters) < 0
                   && !value.Any(char.IsControl);
        }

        /// <summary>
        /// Checks whether a directory in the artifact root is internal and not a product.
        /// </summary>
        public static bool IsInternalDirectory(string name)
        {
            return name.StartsWith(InternalDirectoryPrefix, StringComparison.Ordinal);
        }
    }
}
