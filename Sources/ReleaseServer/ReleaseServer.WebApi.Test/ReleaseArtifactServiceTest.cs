using System.IO;
using Moq;
using release_server_web_api.Services;
using Microsoft.AspNetCore.Http;    
using Xunit;

namespace release_server_web_api_test
{
    public class ReleaseArtifactServiceTest
    {
        private IReleaseArtifactService FsReleaseArtifactService;

        public ReleaseArtifactServiceTest()
        {
            FsReleaseArtifactService = new FsReleaseArtifactService();
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
            var testPath = Path.Combine("./", "1.0", "ubuntu", "amd64");

            //Act
            FsReleaseArtifactService.StoreArtifact("1.0", "ubuntu", "amd64", testFile);
            
            //Assert
            Assert.True(Directory.Exists(testPath));
            Assert.True(File.Exists(Path.Combine(testPath, "test_artifact.zip")));

            //Cleanup
            Directory.Delete("./1.0", true);
        }
    }
}