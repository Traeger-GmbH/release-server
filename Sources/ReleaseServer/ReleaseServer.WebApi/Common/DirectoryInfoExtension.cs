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
    }
}