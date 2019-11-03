using System;
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
            var releaseArtifactService = new FsReleaseArtifactService();
            var mockedRepository = new Mock<FsReleaseArtifactRepository>();
            //mockedRepository.Setup(_ => _.GetInfosByProductName("product1")).Returns(testProductInfos);

            //TODO: Finalize Unit test
        }
    }
}