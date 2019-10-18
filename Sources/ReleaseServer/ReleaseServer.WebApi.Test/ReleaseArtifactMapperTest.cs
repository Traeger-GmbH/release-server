using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace release_server_web_api_test
{
    public class ReleaseArtifactMapperTest
    {
        private readonly IFormFile testFile;
        private readonly ReleaseArtifactModel expectedArtifact;
        
        public ReleaseArtifactMapperTest()
        {
            //Setup
            var testFileMock = new Mock<IFormFile>();

            testFileMock.Setup(_ => _.FileName).Returns("test_artifact.zip");
            testFileMock.Setup(_ => _.ContentType).Returns("application/zip");
            testFileMock.Setup(_ => _.ContentDisposition).Returns("form-data;name=\"\", filename=\"test_artifact.zip\"\"");

            testFile = testFileMock.Object;
            
            expectedArtifact = new ReleaseArtifactModel
            {
                ProductInformation = new ProductInformationModel
                {
                    ProductIdentifier = "product",
                    Version = "1.1",
                    Os = "ubuntu",
                    HwArchitecture = "amd64"
                },
                Payload = testFile
            };
        }
        
        [Fact]
        public void ConvertToReleaseArtifactTest()
        {
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product", "1.1", "ubuntu",
                "amd64", testFile);
            
            testArtifact.Should().BeEquivalentTo(expectedArtifact);
        }
    }
}