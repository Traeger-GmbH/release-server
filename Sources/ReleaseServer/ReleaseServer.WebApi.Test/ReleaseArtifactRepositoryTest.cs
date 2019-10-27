using System.IO;
using FluentAssertions;
using Moq;
using release_server_web_api.Services;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test
{
    public class ReleaseArtifactRepositoryTest
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;

        public ReleaseArtifactRepositoryTest()
        {
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository("TestData");
        }
        
        [Fact]
        public void TestStoringArtifact()
        {
            //Setup
            var testFileMock = new Mock<IFormFile>();

            testFileMock.Setup(_ => _.FileName).Returns("test_artifact.zip");
            testFileMock.Setup(_ => _.ContentType).Returns("application/zip");
            testFileMock.Setup(_ => _.ContentDisposition).Returns("form-data;name=\"\", filename=\"test_artifact.zip\"\"");

            var testFile = testFileMock.Object;
            var testPath = Path.Combine("TestData", "product", "ubuntu", "amd64", "1.1");

            //Act
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testFile);
            
            FsReleaseArtifactRepository.StoreArtifact(testArtifact);
            
            //Assert
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "test_artifact.zip")));

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