using System.IO;
using Moq;
using release_server_web_api.Services;
using Microsoft.AspNetCore.Http;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test
{
    public class ReleaseArtifactRepositoryTest
    {
        private IReleaseArtifactRepository FsReleaseArtifactRepository;

        public ReleaseArtifactRepositoryTest()
        {
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository();
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
            var testPath = Path.Combine("./", "product", "ubuntu", "amd64", "1.1");

            //Act
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "ubuntu",
                "amd64", "1.1", testFile);
            
            FsReleaseArtifactRepository.StoreArtifact(testArtifact);
            
            //Assert
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "test_artifact.zip")));

            //Cleanup
            Directory.Delete("./product", true);
        }
    }
}