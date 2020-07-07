//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactRepositoryTest.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ReleaseArtifactRepositoryTest
    {
        private IReleaseArtifactRepository fsReleaseArtifactRepository;
        private readonly string projectDirectory;
        public ReleaseArtifactRepositoryTest()
        {
            //Could be done smarter
            projectDirectory = TestUtils.GetProjectDirectory();

            var artifactRootDirectory = new DirectoryInfo(Path.Combine(projectDirectory, "TestData"));
            var backupRootDirectory = new DirectoryInfo(Path.Combine(projectDirectory, "TestBackupDir"));

            fsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    artifactRootDirectory,
                    backupRootDirectory
                );
        }
        
        [Fact]
        public void TestStoringArtifact()
        {
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(projectDirectory, "TestData", "product"));

            var testPath = Path.Combine(projectDirectory, "TestData", "product", "ubuntu", "amd64", "1.1");
            
            //Create test artifact from a zip file
            var testFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "test_zip.zip"));
            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testZip);
            
            //Create the update test artifact from a zip file
            var testUpdateFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "update_test_zip.zip"));
            using var updateStream = new MemoryStream(testUpdateFile);
            var testUpdateZip = new ZipArchive(updateStream);
            
            var testUpdateArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testUpdateZip);

            //Act
            
            //########################################
            //1. Store the first artifact -> product folder will be created
            //########################################
            fsReleaseArtifactRepository.StoreArtifact(testArtifact);

            //Assert if the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "releaseNotes.json")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram.exe")));
            Assert.True(File.Exists(Path.Combine(testPath, "deployment.json")));
            
            //########################################
            //2. Update the already existing product with the same artifact -> artifact folder & content will be updated 
            //########################################
            fsReleaseArtifactRepository.StoreArtifact(testUpdateArtifact);

            //Assert if the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "releaseNotes_update.json")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram_update.exe")));
            Assert.True(File.Exists(Path.Combine(testPath, "deployment_update.json")));

            //Cleanup
            //Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "product"), true);
        }

        
        [Fact]
        public void TestGetInfosByProductName()
        {
            //Setup
            var testProductPath = Path.Combine(projectDirectory, "TestData", "testproduct", "debian", "amd64", "1.1");
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(projectDirectory, "TestData", "testproduct"));

            Directory.CreateDirectory(testProductPath);
            
            var test = new DirectoryInfo(Path.Combine(projectDirectory, "TestData", "productx", "debian", "amd64", "1.1"));

            foreach (var file in test.GetFiles())
            {
                file.CopyTo(Path.Combine(testProductPath, file.Name));
            }

            var expectedProductInfo = new ProductInformation
            {
                Identifier = "testproduct",
                Os = "debian",
                Architecture = "amd64",
                Version = new ProductVersion("1.1"),
                ReleaseNotes = new ReleaseNotes
                {
                    Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {
                            new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added de 1"}
                                }
                            }
                        },
                        {
                            new CultureInfo("en"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added en 1"}
                                }
                            }
                        }
                    }
                }
            };

            //Act
            var testProductInfo = fsReleaseArtifactRepository.GetInfosByProductName("testproduct");
            
            //Assert
            testProductInfo.Should().BeEquivalentTo(expectedProductInfo);

            //Cleanup
            Directory.Delete(Path.Combine(projectDirectory, "TestData", "testproduct"), true);
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
            var testPlatforms1 = fsReleaseArtifactRepository.GetPlatforms("productx", "1.0");
            var testPlatforms2 = fsReleaseArtifactRepository.GetPlatforms("productx", "1.1");
            var testPlatforms3 = fsReleaseArtifactRepository.GetPlatforms("productx", "1.2-beta");

            //Assert
            Assert.Equal(expectedPlatforms1, testPlatforms1);
            Assert.Equal(expectedPlatforms2, testPlatforms2);
            Assert.Equal(expectedPlatforms3, testPlatforms3);
        }
        
        [Fact]
        public void TestGetVersions()
        {
            //Setup 
            var expectedVersions1 = new List<ProductVersion>
                {new ProductVersion("1.2-beta"), new ProductVersion("1.1")};

            var expectedVersions2 = new List<ProductVersion>
                { new ProductVersion("1.1"), new ProductVersion("1.0")};
            
            var expectedVersions3 = new List<ProductVersion>
                {new ProductVersion("1.0")};

            
            //Act
            var testVersions1 = fsReleaseArtifactRepository.GetVersions("productx", "debian", "amd64");
            var testVersions2 = fsReleaseArtifactRepository.GetVersions("productx", "ubuntu", "amd64");
            var testVersions3 = fsReleaseArtifactRepository.GetVersions("productx", "ubuntu", "arm64");

            //Assert
            Assert.Equal(expectedVersions1, testVersions1);
            Assert.Equal(expectedVersions2, testVersions2);
            Assert.Equal(expectedVersions3, testVersions3);
        }

        [Fact]
        public void TestGetReleaseInfo()
        {
            //Setup 
            ReleaseInformation expectedReleaseInfo;
            
            expectedReleaseInfo = new ReleaseInformation
            {
                ReleaseDate = new DateTime(2020, 02, 10),
                
                ReleaseNotes = new ReleaseNotes
                {
                    
                    Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added de 1", "added de 2"},
                                    Fixed = new List<string>{"fix de 1", "fix de 2"},
                                    Breaking = new List<string>{"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string>{"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        },
                        {new CultureInfo("en"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string>{"windows/any", "linux/rpi"},
                                    Added = new List<string>{"added en 1", "added en 2"},
                                    Fixed = new List<string>{"fix en 1", "fix en 2"},
                                    Breaking = new List<string>{"breaking en 1", "breaking en 2"},
                                    Deprecated = new List<string>{"deprecated en 1", "deprecated en 2"}
                                },
                                new ChangeSet
                                {
                                    Platforms = null,
                                    Added = new List<string>{"added en 3"},
                                    Fixed = new List<string>{"fix en 3"},
                                    Breaking = new List<string>{"breaking en 3"},
                                    Deprecated = new List<string>{"deprecated en 3"}
                                }
                            }
                        }
                    }
                }
            };
            
            //Act
            var testReleaseInfo = fsReleaseArtifactRepository.GetReleaseInfo("productx", "ubuntu",
                "amd64", "1.1");
            
            //Assert 
            testReleaseInfo.Should().BeEquivalentTo(expectedReleaseInfo);
        }
        
        [Fact]
        public void TestGetReleaseInfo_Not_Found()
        {
            //Act
            var testReleaseInfo = fsReleaseArtifactRepository.GetReleaseInfo("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert 
            Assert.Null(testReleaseInfo);
        }

        [Fact]
        public void TestGetSpecificArtifact()
        {
            //Setup
            var expectedArtifact = new ArtifactDownload
            {
                FileName = Path.GetFileName(Path.Combine(projectDirectory, "TestData", "productx",
                    "ubuntu", "amd64", "1.1", "artifact.zip")),

                Payload = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "productx",
                    "ubuntu", "amd64", "1.1", "artifact.zip"))
            };

            //Act
            var testArtifact = fsReleaseArtifactRepository.GetSpecificArtifact("productx",
                "ubuntu", "amd64", "1.1");
            
            //Assert
            testArtifact.Should().BeEquivalentTo(expectedArtifact);
        }
        
        [Fact]
        public void TestGetSpecificArtifact_Not_Found()
        {
            //Act
            var testArtifact = fsReleaseArtifactRepository.GetSpecificArtifact("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert
            Assert.Null(testArtifact);
        }

        [Fact]
        public void TestDeleteSpecificArtifact()
        {
            //Setup
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(projectDirectory, "TestData", "producty"));
            
            TestUtils.SetupTestDirectory(projectDirectory);
            
            //Act 
            var productFound = fsReleaseArtifactRepository.DeleteSpecificArtifactIfExists("producty", "ubuntu", "amd64", "1.1");
            
            //Assert
            Assert.True(productFound);
            Assert.False(Directory.Exists(Path.Combine(projectDirectory, "TestData", "producty", "ubuntu", "amd64", "1.1")));
            
            //Cleanup
            Directory.Delete(Path.Combine(projectDirectory, "TestData", "producty"), true);
        }
        
        [Fact]
        public void TestDeleteSpecificArtifact_Not_Found()
        {
            //Act 
            var productFound = fsReleaseArtifactRepository.DeleteSpecificArtifactIfExists("nonexistentProduct", "noOS", "noArch", "0");
            
            //Assert
            Assert.False(productFound);
        }
        
        [Fact]
        public void TestDeleteProduct()
        {
            //Setup
            
            //Cleanup test dir from old tests (if they failed before)
            TestUtils.CleanupDirIfExists(Path.Combine(projectDirectory, "TestData", "producty"));
            
            TestUtils.SetupTestDirectory(projectDirectory);
            
            //Act 
            var artifactFound = fsReleaseArtifactRepository.DeleteProductIfExists("producty");
            
            //Assert
            Assert.True(artifactFound);
            Assert.False(Directory.Exists(Path.Combine(projectDirectory, "TestData", "producty")));
        }
        
        [Fact]
        public void TestDeleteProductI_Not_Found()
        {
            //Act 
            var artifactFound = fsReleaseArtifactRepository.DeleteProductIfExists("nonexistentProduct");
            
            //Assert
            Assert.False(artifactFound); }

        [Fact]
        public void TestRunBackup()
        {
            //Setup
            var testArtifactRoot = new DirectoryInfo(Path.Combine(projectDirectory, "TestArtifactRoot"));
            var testBackupDir = new DirectoryInfo(Path.Combine(projectDirectory, "TestBackup"));
            

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
            
            var expectedFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "test_zip.zip"));

            //Copy to the test file to the custom ArtifactRootDirectory
            File.Copy(Path.Combine(projectDirectory, "TestData", "test_zip.zip"), Path.Combine(testArtifactRoot.FullName, "test_zip.zip"));
            
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
            var testArtifactRoot = new DirectoryInfo(Path.Combine(projectDirectory, "TestArtifactRoot"));
            var testBackupDir = new DirectoryInfo(Path.Combine(projectDirectory, "TestBackup"));

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
            
            var testBackupZip = new ZipArchive(File.OpenRead(Path.Combine(projectDirectory, "TestData", "restoreTest", "testFile.zip")));
            var expectedFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "restoreTest", "testFile.txt"));
            
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