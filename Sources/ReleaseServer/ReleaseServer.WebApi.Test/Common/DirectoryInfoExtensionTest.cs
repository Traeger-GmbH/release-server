using System;
using System.IO;
using System.Text;
using ReleaseServer.WebApi.Common;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test.Common
{
    public class DirectoryInfoExtensionTest
    {
        private readonly string ProjectDirectory;

        public DirectoryInfoExtensionTest()
        {
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
        }

        [Fact]
        public void DeleteFileContent()
        {
            //Setup
            var testDir = Path.Combine(ProjectDirectory, "TestDirectoryForDeletion");
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(testDir));
            
            var testDirInfo = Directory.CreateDirectory(testDir);
            
            //Create test data (test file & test subdirectories)
            using (var fs = File.Create(Path.Combine(testDirInfo.ToString(), "testFile")))
            {
                byte[] info = new UTF8Encoding(true).GetBytes("This is a test file.");
                fs.Write(info, 0, info.Length);
            }

            Directory.CreateDirectory(Path.Combine(testDirInfo.ToString(), "testSubDir1"));
            Directory.CreateDirectory(Path.Combine(testDirInfo.ToString(), "testSubDir2"));
            
            //Act
            testDirInfo.DeleteContent();

            //Assert
            Assert.Empty(testDirInfo.GetFiles());
            Assert.Empty(testDirInfo.GetDirectories());
            
            //Cleanup
            testDirInfo.Delete(true);
        }
    }
}