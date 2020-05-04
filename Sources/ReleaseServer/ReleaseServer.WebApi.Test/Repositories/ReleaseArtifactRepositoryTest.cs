using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Test.Utils;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ReleaseArtifactRepositoryTest
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private readonly string ProjectDirectory;
        public ReleaseArtifactRepositoryTest()
        {
            //Could be done smarter
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            var artifactRootDirectory = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestData"));
            var backupRootDirectory = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestBackupDir"));

            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    artifactRootDirectory,
                    backupRootDirectory
                );
        }
        
        [Fact]
        public void TestStoringArtifact()
        {
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(ProjectDirectory, "TestData", "product"));

            var testPath = Path.Combine(ProjectDirectory, "TestData", "product", "ubuntu", "amd64", "1.1");
            
            //Create test artifact from a zip file
            var testFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "test_zip.zip"));
            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testZip);
            
            //Create the update test artifact from a zip file
            var testUpdateFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "update_test_zip.zip"));
            using var updateStream = new MemoryStream(testUpdateFile);
            var testUpdateZip = new ZipArchive(updateStream);
            
            var testUpdateArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testUpdateZip);

            //Act
            
            //########################################
            //1. Store the first artifact -> product folder will be created
            //########################################
            FsReleaseArtifactRepository.StoreArtifact(testArtifact);

            //Assert if the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "changelog.txt")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram.exe")));
            Assert.True(File.Exists(Path.Combine(testPath, "deployment.json")));
            
            //########################################
            //2. Update the already existing product with the same artifact -> artifact folder & content will be updated 
            //########################################
            FsReleaseArtifactRepository.StoreArtifact(testUpdateArtifact);

            //Assert if the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "changelog_update.txt")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram_update.exe")));
            Assert.True(File.Exists(Path.Combine(testPath, "deployment_update.json")));

            //Cleanup
            //Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "product"), true);
        }

        [Fact]
        public void TestGetInfosByProductName()
        {
            //Setup
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(ProjectDirectory, "TestData", "product"));
            
            Directory.CreateDirectory(Path.Combine(ProjectDirectory, "TestData", "testproduct", "debian", "amd64", "1.0"));

            var expectedProductInfo = new ProductInformationModel
            {
                ProductIdentifier = "testproduct",
                Os = "debian",
                HwArchitecture = "amd64",
                Version = "1.0".ToProductVersion(),
            };

            //Act
            var testProductInfo = FsReleaseArtifactRepository.GetInfosByProductName("testproduct");
            
            //Assert
            testProductInfo.Should().BeEquivalentTo(expectedProductInfo);

            //Cleanup
            Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "testproduct"), true);
        }
        
        [Fact]
        public void TestGetPlatforms()
        {
            //Setup
            List<string> expectedPlatforms1 = new List<string>()
                {"ubuntu-amd64", "ubuntu-arm64"};
            
            List<string> expectedPlatforms2 = new List<string>()
                {"debian-amd64", "ubuntu-amd64"};
            
            List<string> expectedPlatforms3 = new List<string>()
                {"debian-amd64"};

            //Act
            var testPlatforms1 = FsReleaseArtifactRepository.GetPlatforms("productx", "1.0");
            var testPlatforms2 = FsReleaseArtifactRepository.GetPlatforms("productx", "1.1");
            var testPlatforms3 = FsReleaseArtifactRepository.GetPlatforms("productx", "1.2-beta");

            //Assert
            Assert.Equal(expectedPlatforms1, testPlatforms1);
            Assert.Equal(expectedPlatforms2, testPlatforms2);
            Assert.Equal(expectedPlatforms3, testPlatforms3);
        }
        
        [Fact]
        public void TestGetVersions()
        {
            //Setup 
            List<string> expectedVersions1 = new List<string>()
                {"1.2-beta", "1.1"};

            List<string> expectedVersions2 = new List<string>()
            {
                "1.1", "1.0"
            };
            
            List<string> expectedVersions3 = new List<string>()
            {
                "1.0"
            };
            
            //Act
            var testVersions1 = FsReleaseArtifactRepository.GetVersions("productx", "debian", "amd64");
            var testVersions2 = FsReleaseArtifactRepository.GetVersions("productx", "ubuntu", "amd64");
            var testVersions3 = FsReleaseArtifactRepository.GetVersions("productx", "ubuntu", "arm64");

            //Assert
            Assert.Equal(expectedVersions1, testVersions1);
            Assert.Equal(expectedVersions2, testVersions2);
            Assert.Equal(expectedVersions3, testVersions3);
        }

        [Fact]
        public void TestGetReleaseInfo()
        {
            //Setup 
            ReleaseInformationModel expectedReleaseInfo;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                expectedReleaseInfo = new ReleaseInformationModel
                {
                    Changelog = "Release 1.0.0\r\n- This is an example\r\n- This is another example",
                    ReleaseDate = new DateTime(2020, 02, 10)
                };
            }
            else
            {
                expectedReleaseInfo = new ReleaseInformationModel
                {
                    Changelog = "Release 1.0.0\n- This is an example\n- This is another example",
                    ReleaseDate = new DateTime(2020, 02, 10)
                };

            }
            
            //Act
            var testReleaseInfo = FsReleaseArtifactRepository.GetReleaseInfo("productx", "ubuntu",
                "amd64", "1.1");
            
            //Assert 
            testReleaseInfo.Should().BeEquivalentTo(expectedReleaseInfo);
        }
        
        [Fact]
        public void TestGetReleaseInfo_Not_Found()
        {
            //Act
            var testReleaseInfo = FsReleaseArtifactRepository.GetReleaseInfo("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert 
            Assert.Null(testReleaseInfo);
        }

        [Fact]
        public void TestGetSpecificArtifact()
        {
            //Setup
            var expectedArtifact = new ArtifactDownloadModel
            {
                FileName = Path.GetFileName(Path.Combine(ProjectDirectory, "TestData", "productx",
                    "ubuntu", "amd64", "1.1", "artifact.zip")),

                Payload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "productx",
                    "ubuntu", "amd64", "1.1", "artifact.zip"))
            };

            //Act
            var testArtifact = FsReleaseArtifactRepository.GetSpecificArtifact("productx",
                "ubuntu", "amd64", "1.1");
            
            //Assert
            testArtifact.Should().BeEquivalentTo(expectedArtifact);
        }
        
        [Fact]
        public void TestGetSpecificArtifact_Not_Found()
        {
            //Act
            var testArtifact = FsReleaseArtifactRepository.GetSpecificArtifact("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert
            Assert.Null(testArtifact);
        }

        [Fact]
        public void TestDeleteSpecificArtifact()
        {
            //Setup
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(ProjectDirectory, "TestData", "producty"));
            
            TestUtils.SetupTestDirectory(ProjectDirectory);
            
            //Act 
            var productFound = FsReleaseArtifactRepository.DeleteSpecificArtifactIfExists("producty", "ubuntu", "amd64", "1.1");
            
            //Assert
            Assert.True(productFound);
            Assert.False(Directory.Exists(Path.Combine(ProjectDirectory, "TestData", "producty", "ubuntu", "amd64", "1.1")));
            
            //Cleanup
            Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "producty"), true);
        }
        
        [Fact]
        public void TestDeleteSpecificArtifact_Not_Found()
        {
            //Act 
            var productFound = FsReleaseArtifactRepository.DeleteSpecificArtifactIfExists("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert
            Assert.False(productFound);
        }
        
        [Fact]
        public void TestDeleteProduct()
        {
            //Setup
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(ProjectDirectory, "TestData", "producty"));
            
            TestUtils.SetupTestDirectory(ProjectDirectory);
            
            //Act 
            var artifactFound = FsReleaseArtifactRepository.DeleteProductIfExists("producty");
            
            //Assert
            Assert.True(artifactFound);
            Assert.False(Directory.Exists(Path.Combine(ProjectDirectory, "TestData", "producty")));
        }
        
        [Fact]
        public void TestDeleteProductI_Not_Found()
        {
            //Act 
            var artifactFound = FsReleaseArtifactRepository.DeleteProductIfExists("nonexistentProduct");
            
            //Assert
            Assert.False(artifactFound); }

        [Fact]
        public void TestRunBackup()
        {
            //Setup
            var testArtifactRoot = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestArtifactRoot"));
            var testBackupDir = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestBackup"));
            

            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(testArtifactRoot.FullName);
            TestUtils.CleanupDirIfExists(testBackupDir.FullName);

            if (!testArtifactRoot.Exists) {
                testArtifactRoot.Create();
            }
            if (!testBackupDir.Exists) {
                testBackupDir.Create();
            }

            var customRepository = new FsReleaseArtifactRepository(
                Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                testArtifactRoot,
                testBackupDir
            );
            
            var expectedFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "test_zip.zip"));

            //Copy to the test file to the custom ArtifactRootDirectory
            File.Copy(Path.Combine(ProjectDirectory, "TestData", "test_zip.zip"), Path.Combine(testArtifactRoot.FullName, "test_zip.zip"));
            
            //Act
            customRepository.RunBackup();
            
            var backupFiles = Directory.GetFiles(testBackupDir.FullName);
            
            //There is only one file -> .First() is used
            ZipFile.ExtractToDirectory(backupFiles.First(), testBackupDir.FullName);
            File.Delete(backupFiles.First());
            
            //Get the actual file in the directory
            backupFiles = Directory.GetFiles(testBackupDir.FullName);
            var backupFile = File.ReadAllBytes(backupFiles.First());
            
            //Assert
            Assert.Equal(expectedFile, backupFile);
            
            //Cleanup
            testArtifactRoot.Delete(true);
            testBackupDir.Delete(true);
        }


        [Fact]
        public void TestRestore()
        {
            var testArtifactRoot = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestArtifactRoot"));
            var testBackupDir = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestBackup"));

            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(testArtifactRoot.FullName);
            TestUtils.CleanupDirIfExists(testBackupDir.FullName);

            if (!testArtifactRoot.Exists) {
                testArtifactRoot.Create();
            }
            if (!testBackupDir.Exists) {
                testBackupDir.Create();
            }

            var customRepository = new FsReleaseArtifactRepository(
                Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                testArtifactRoot,
                testBackupDir
            );
            
            var testBackupZip = new ZipArchive(File.OpenRead(Path.Combine(ProjectDirectory, "TestData", "restoreTest", "testFile.zip")));
            var expectedFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "restoreTest", "testFile.txt"));
            
            //Act
            customRepository.RestoreBackup(testBackupZip);
            
            //Get the (only) one file in the testArtifactRootDirectory
            var artifactFiles = Directory.GetFiles(testArtifactRoot.FullName);
            var testFile = File.ReadAllBytes(artifactFiles.First());
            
            //Assert
            Assert.Equal(expectedFile, testFile);
            
            //Cleanup
            testArtifactRoot.Delete(true);
            testBackupDir.Delete(true);
        }
    }
}