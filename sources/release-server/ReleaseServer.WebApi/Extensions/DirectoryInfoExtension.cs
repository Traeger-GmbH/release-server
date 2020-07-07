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
    public static class DirectoryInfoExtension
    {
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

        public static void Move(this DirectoryInfo directory, string path, bool overwriteExisting = false)
        {
            var destination = new DirectoryInfo(path);
            directory.Move(destination, overwriteExisting);
        }

        public static void Move(this DirectoryInfo directory, DirectoryInfo destination, bool overwriteExisting = false)
        {
            destination.Refresh();
            if (destination.Exists)
            {
                if (overwriteExisting)
                {
                    destination.Delete(true);
                }
                else
                {
                    throw new IOException($"Could not move directory: New path {destination.FullName} already exists.");
                }
            }

            Directory.Move(directory.FullName, destination.FullName);
        }
        
        public static void EnsureWritable(this DirectoryInfo directory) {            
            if (!directory.IsWritable())
            {
                throw new UnauthorizedAccessException($"Unable to write to the directory {directory.FullName}. Please check your permissions!");
            }
        }
        
        public static bool IsWritable(this DirectoryInfo directory)
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