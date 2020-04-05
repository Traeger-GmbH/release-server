using System;
using System.IO;

namespace ReleaseServer.WebApi.Common
{
    public static class FileSystemPermissions
    {
        public static bool CanWriteDirectory(string path)
        {
            bool writable = false;

            try {
                path = Path.GetFullPath(path);

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