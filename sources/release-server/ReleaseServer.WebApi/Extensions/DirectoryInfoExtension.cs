//--------------------------------------------------------------------------------------------------
// <copyright file="DirectoryInfoExtension.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// An extension class for operations with objects of the <see cref="DirectoryInfo"/> class.
    /// </summary>
    public static class DirectoryInfoExtension
    {
        /// <summary>
        /// Deletes all files of the given directory.
        /// </summary>
        /// <param name="directory">The <see cref="DirectoryInfo"/> of the directory that has to be cleaned up.</param>
        public static void DeleteContent(this DirectoryInfo directory)
        {
            if (!directory.Exists) return;
            
            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (var subDir in directory.GetDirectories())
            {
                subDir.Delete(true);
            }
        }

        /// <summary>
        /// Moves the content of the source directory (given as <see cref="DirectoryInfo"/>) to the destination
        /// directory (given as string).
        /// </summary>
        /// <param name="sourceDirectory">The <see cref="DirectoryInfo"/> of the source directory.</param>
        /// <param name="destinationPath">The path of the destination directory.</param>
        /// <param name="overwriteExisting">Option for overwriting the existing content in the destination directory.
        /// The existing content will be overwritten, if the option is set to true (default: false).</param>
        public static void Move(this DirectoryInfo sourceDirectory, string destinationPath, bool overwriteExisting = false)
        {
            var destination = new DirectoryInfo(destinationPath);
            sourceDirectory.Move(destination, overwriteExisting);
        }

        /// <summary>
        /// Moves the content of the source directory (given as <see cref="DirectoryInfo"/>) to the destination
        /// directory (given as <see cref="DirectoryInfo"/>).
        /// </summary>
        /// <param name="sourceDirectory">The <see cref="DirectoryInfo"/> of the source directory.</param>
        /// <param name="destinationDirectory">The <see cref="DirectoryInfo"/> of the destination directory.</param>
        /// <param name="overwriteExisting">Option for overwriting the existing content in the destination directory.
        /// The existing content will be overwritten, if the option is set to true (default: false).</param>
        /// <exception cref="IOException">Will be thrown, if the destination directory already exists.</exception>
        public static void Move(this DirectoryInfo sourceDirectory, DirectoryInfo destinationDirectory, bool overwriteExisting = false)
        {
            destinationDirectory.Refresh();
            if (destinationDirectory.Exists)
            {
                if (overwriteExisting)
                {
                    destinationDirectory.Delete(true);
                }
                else
                {
                    throw new IOException($"Could not move directory: New path {destinationDirectory.FullName} already exists.");
                }
            }

            Directory.Move(sourceDirectory.FullName, destinationDirectory.FullName);
        }
        
        /// <summary>
        /// Ensures if it's possible to write to the specified directory and throws an exception, if not.
        /// </summary>
        /// <param name="directory">The <see cref="DirectoryInfo"/> to the directory, that has to be checked.</param>
        /// <exception cref="UnauthorizedAccessException">Will be thrown, if it's unable to write to the directory.</exception>
        public static void EnsureWritable(this DirectoryInfo directory) {            
            if (!directory.IsWritable())
            {
                throw new UnauthorizedAccessException($"Unable to write to the directory {directory.FullName}. Please check your permissions!");
            }
        }
        
        private static bool IsWritable(this DirectoryInfo directory)
        {
            bool writable = false;
            
            try {
                var path = directory.FullName;

                if (Directory.Exists(path)) {
                    // Generate a file name to reduce the risk of a file that might already exist.
                    path = Path.Combine(path, Guid.NewGuid().ToString("N") + "_test.file");

                    // It is important that we use FileMode.CreateNew instead of Create; otherwise
                    // we might overwrite an already existing file. Also, we need to use
                    // FileShare.Delete() so that we can delete the file while we have opened it,
                    // to ensure no other process can write data to that file between the time when
                    // we close it and then delete it.
                    using (var stream = new FileStream(
                        path,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.Delete)) {
                        // Delete it before closing it, to ensure no other process can access it.
                        File.Delete(path);
                    }

                    writable = true;
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch {
                // Ignore.
            }
#pragma warning restore CA1031 // Do not catch general exception types

            return writable;
        }
    }
}