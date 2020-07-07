//--------------------------------------------------------------------------------------------------
// <copyright file="DirectoryInfoExtensionTest.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;
using Xunit;

namespace ReleaseServer.WebApi.Test.Extensions
{
    public class DirectoryInfoExtensionTest
    {
        private readonly string projectDirectory;

        public DirectoryInfoExtensionTest()
        {
            projectDirectory = TestUtils.GetProjectDirectory();
        }

        [Fact]
        public void DeleteFileContent()
        {
            //Setup
            var testDir = Path.Combine(projectDirectory, "TestDirectoryForDeletion");
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(testDir));
            
            var testDirInfo = Directory.CreateDirectory(testDir);
            
            TestUtils.CreateTestFile(testDir);

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

        [Fact]
        public void MoveDirectory_Success()
        {
            //Setup
            var testDestinationDir = Path.Combine(projectDirectory, "TestDestinationDir");
            var testSourceDir = Path.Combine(projectDirectory, "TestSourceDir");
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(testDestinationDir);
            TestUtils.CleanupDirIfExists(testSourceDir);
            
            var testSourceDirInfo = Directory.CreateDirectory(testSourceDir);
            var testDestinationDirInfo = Directory.CreateDirectory(testDestinationDir);
            
            var testFilePath = TestUtils.CreateTestFile(testSourceDirInfo.ToString());

            var bytesBeforeMoving = File.ReadAllBytes(testFilePath);
            
            //Act
            //Use the Move() function with the string argument so that we can test the all Move() implementations.
            testSourceDirInfo.Move(testDestinationDirInfo.ToString(), true);

            var bytesAfterMoving = File.ReadAllBytes(Path.Combine(testDestinationDirInfo.ToString(), "testFile"));

            Assert.Equal(bytesBeforeMoving,bytesAfterMoving);
            
            //Cleanup
            Directory.Delete(testDestinationDir, true);
        }

        [Fact]
        public void MoveDirectory_Error()
        {
            //Setup
            var testDestinationDir = Path.Combine(projectDirectory, "TestDestinationDir");
            var testSourceDir = Path.Combine(projectDirectory, "TestSourceDir");
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(testDestinationDir);
            TestUtils.CleanupDirIfExists(testSourceDir);
            
            var testSourceDirInfo = Directory.CreateDirectory(testSourceDir);
            var testDestinationDirInfo = Directory.CreateDirectory(testDestinationDir);
            
            TestUtils.CreateTestFile(testSourceDirInfo.ToString());
            
            //Act & Assert
            //Use the Move() function with the string argument so that we can test the all Move() implementations.
            Assert.Throws<IOException>(() => testSourceDirInfo.Move(testDestinationDirInfo.ToString()));
            
            //Cleanup
            Directory.Delete(testSourceDir, true);            
            Directory.Delete(testDestinationDir, true);
        }
    }
}