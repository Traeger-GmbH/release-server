//--------------------------------------------------------------------------------------------------
// <copyright file="ZipArchiveExtensions.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Extensions for operations on ZipArchive class.
    /// </summary>
    public static class ZipArchiveExtensions
    {
        /// <summary>
        /// Gets an entry of a zip archive specified by the entry name.
        /// </summary>
        /// <param name="archive"></param>
        /// <param name="entryName"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static bool TryGetEntry(this ZipArchive archive, string entryName, out ZipArchiveEntry entry)
        {
            entry = archive.GetEntry(entryName);

            if (entry != null) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}