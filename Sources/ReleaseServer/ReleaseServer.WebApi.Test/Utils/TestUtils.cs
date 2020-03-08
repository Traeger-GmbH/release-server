using System.IO;

namespace ReleaseServer.WebApi.Test.Utils
{
    public static class TestUtils
    {
        public static void CleanupDirIfExists(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }
        
        
        public static void SetupTestDirectory(string rootDirectory)
        {
            //Setup
            var sourcePath = Path.Combine(rootDirectory, "TestData", "productx", "ubuntu", "amd64", "1.1");
            Directory.CreateDirectory(Path.Combine(rootDirectory, "TestData", "producty", "ubuntu", "amd64", "1.1"));

            var files = Directory.GetFiles(sourcePath);

            foreach (var file in files)
            {
                var destFile = file.Replace("productx", "producty");
                File.Copy(file, destFile);
            }
        }
        
    }
}