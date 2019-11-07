using System;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
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
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository("TestData");
            //Could be done smarter
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

        }
        
        [Fact]
        public async void TestStoringArtifact()
        {

            var testFile = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "test_zip.zip"));

            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            var testPath = Path.Combine("TestData", "product", "ubuntu", "amd64", "1.1");
                    
            //Act
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testZip);

            await FsReleaseArtifactRepository.StoreArtifact(testArtifact);

            //Assert whether the directory and the unzipped files exist
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "changelog.txt")));
            Assert.True(File.Exists(Path.Combine(testPath, "testprogram.exe")));

            //Cleanup
            Directory.Delete("TestData", true);
        }

        [Fact]
        public void TestGetInfosByProductName()
        {
            
            //Setup
            Directory.CreateDirectory(Path.Combine("TestData", "testproduct", "debian", "amd64", "1.0"));
            Directory.CreateDirectory(Path.Combine("TestData", "testproduct", "debian", "amd64"));
            
            var expectedProductInfo = new ProductInformationModel
            {
                ProductIdentifier = "testproduct",
                Os = "debian",
                HwArchitecture = "amd64",
                Version = "1.0",
            };

            //Act
            var testProductInfo = FsReleaseArtifactRepository.GetInfosByProductName("testproduct");
            
            //Assert
            testProductInfo.Should().BeEquivalentTo(expectedProductInfo);

            //Cleanup
            Directory.Delete("TestData", true);
        }
    }
}