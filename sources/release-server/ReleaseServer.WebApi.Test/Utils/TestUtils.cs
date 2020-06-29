using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
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
        
        public static byte[] CreateTestZipFile(List<string> filePathList)
        {
            using var memoryStream = new MemoryStream();
            using (var zipArchive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var filePath in filePathList)
                {
                    var fileName = Path.GetFileName(filePath);
                    var fileBytes = File.ReadAllBytes(Path.Combine(filePath, filePath));
                    
                    var zipArchiveEntry = zipArchive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using (var zipStream = zipArchiveEntry.Open()) zipStream.Write(fileBytes, 0, fileBytes.Length);
                }
            }

            return memoryStream.ToArray();
        }
    }
}