using System;
using System.IO;
using System.Text;

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
        
        public static string CreateTestFile(string destinationPath)
        {
            var testDirInfo = new DirectoryInfo(destinationPath);
            
            //Create test data (test file & test subdirectories)
            using (var fs = File.Create(Path.Combine(testDirInfo.ToString(), "testFile")))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is a test file.");
                fs.Write(info, 0, info.Length);
            }

            return Path.Combine(destinationPath.ToString(), "testFile");
        }

        public static string GetProjectDirectory()
        {
            return Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        }
    }
}