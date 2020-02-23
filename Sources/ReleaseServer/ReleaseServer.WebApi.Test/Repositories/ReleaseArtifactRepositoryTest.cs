using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
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

            var configuration = new Mock<IConfiguration>();
            configuration
                .SetupGet(x => x[It.Is<string>(s => s == "ArtifactRootDirectory")])
                .Returns(Path.Combine(ProjectDirectory, "TestData"));

            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    configuration.Object
                );
        }
        
        [Fact]
        public async void TestStoringArtifact()
        {

            var testFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "test_zip.zip"));

            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            var testPath = Path.Combine(ProjectDirectory, "TestData", "product", "ubuntu", "amd64", "1.1");
                    
            //Act
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testZip);

            FsReleaseArtifactRepository.StoreArtifact(testArtifact);

            //Assert whether the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "changelog.txt")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram.exe")));

            //Cleanup
            Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "product"), true);
        }

        [Fact]
        public void TestGetInfosByProductName()
        {
            
            //Setup
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
            string expectedReleaseInfo;
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                expectedReleaseInfo = "Release 1.0.0\r\n- This is an example\r\n- This is another example";
            }
            else
            {
                expectedReleaseInfo = "Release 1.0.0\n- This is an example\n- This is another example";
            }
            
            //Act
            var testReleaseInfo = FsReleaseArtifactRepository.GetReleaseInfo("productx", "ubuntu",
                "amd64", "1.1");
            
            //Assert 
            testReleaseInfo.Should().BeEquivalentTo(expectedReleaseInfo);
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
        public void TestDeleteSpecificArtifact()
        {
            //Setup
            SetupTestDirectory();
            
            //Act 
            FsReleaseArtifactRepository.DeleteSpecificArtifact("producty", "ubuntu", "amd64", "1.1");
            
            //Assert
            Assert.False(Directory.Exists(Path.Combine(ProjectDirectory, "TestData", "producty", "ubuntu", "amd64", "1.1")));
            
            //Cleanup
            Directory.Delete(Path.Combine(ProjectDirectory, "TestData", "producty"), true);
        }
        
        [Fact]
        public void TestDeleteProduct()
        {
            //Setup
            SetupTestDirectory();
            
            //Act 
            FsReleaseArtifactRepository.DeleteProduct("producty");
            
            //Assert
            Assert.False(Directory.Exists(Path.Combine(ProjectDirectory, "TestData", "producty")));
        }

        private void SetupTestDirectory()
        {
            //Setup
            var sourcePath = Path.Combine(ProjectDirectory, "TestData", "productx", "ubuntu", "amd64", "1.1");
            Directory.CreateDirectory(Path.Combine(ProjectDirectory, "TestData", "producty", "ubuntu", "amd64", "1.1"));

            var files = Directory.GetFiles(sourcePath);

            foreach (var file in files)
            {
                var destFile = file.Replace("productx", "producty");
                File.Copy(file, destFile);
            }
        }
    }
}