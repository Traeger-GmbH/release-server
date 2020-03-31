using System.IO;

namespace ReleaseServer.WebApi.Common
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
    }
}