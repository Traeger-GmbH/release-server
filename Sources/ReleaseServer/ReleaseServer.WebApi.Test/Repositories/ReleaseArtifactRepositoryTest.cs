using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test
{
    public class ReleaseArtifactRepositoryTest
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private readonly string ProjectDirectory;

        public ReleaseArtifactRepositoryTest()
        {
            //Could be done smarter
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName; 
            
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(Substitute.For<ILogger<FsReleaseArtifactRepository>>(),Path.Combine(ProjectDirectory, "TestData"));
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

            await FsReleaseArtifactRepository.StoreArtifact(testArtifact);

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
        public void TestGetReleaseInfo()
        {
            //Setup 
            var expectedReleaseInfo = "Release 1.0.0\r\n- This is an example\r\n- This is another example";
            
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