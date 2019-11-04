using System.Collections.Generic;
using Moq;
using release_server_web_api.Services;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test.TestData
{
    public class ReleaseArtifactServiceTest
    {
        private IReleaseArtifactService FsReleaseArtifactService;
        private readonly List<ProductInformationModel> testProductInfos;

        public ReleaseArtifactServiceTest()
        {
            FsReleaseArtifactService = new FsReleaseArtifactService(new FsReleaseArtifactRepository());

            testProductInfos = new List<ProductInformationModel>()
            {

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "ubuntu",
                    HwArchitecture = "arm64",
                    Version = "1.0"
                },

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "ubuntu",
                    HwArchitecture = "amd64",
                    Version = "1.0"
                },

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "debian",
                    HwArchitecture = "amd64",
                    Version = "1.1"
                }
            };
        }

        [Fact]
        public void TestGetPlatforms()
        {
            //Setup
            List<string> expectedPlatforms1 = new List<string>()
                {"ubuntu-arm64", "ubuntu-amd64"};
            
            List<string> expectedPlatforms2 = new List<string>()
                {"debian-amd64"};

           var repositoryMock = new Mock<IReleaseArtifactRepository>();
            repositoryMock.Setup(r => r.GetInfosByProductName("product1")).Returns(new List<ProductInformationModel>(testProductInfos));
            var mockedRepository = repositoryMock.Object;
            
            var releaseArtifactService = new FsReleaseArtifactService(mockedRepository);

            //Act
            var testPlatforms1 = releaseArtifactService.GetPlatforms("product1", "1.0");
            var testPlatforms2 = releaseArtifactService.GetPlatforms("product1", "1.1");

            //Assert
            Assert.Equal(expectedPlatforms1, testPlatforms1);
            Assert.Equal(expectedPlatforms2, testPlatforms2);
        }
    }
}