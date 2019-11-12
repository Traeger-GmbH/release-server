using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using release_server_web_api.Services;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test.TestData
{
    public class ReleaseArtifactServiceTest
    {
        private readonly List<ProductInformationModel> testProductInfos;

        public ReleaseArtifactServiceTest()
        {
            testProductInfos = new List<ProductInformationModel>()
            {

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "ubuntu",
                    HwArchitecture = "arm64",
                    Version = new Version("1.0")
                },

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "ubuntu",
                    HwArchitecture = "amd64",
                    Version = new Version("1.0")
                },
                
                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "debian",
                    HwArchitecture = "amd64",
                    Version = new Version("1.2")
                },

                new ProductInformationModel
                {
                    ProductIdentifier = "product1",
                    Os = "debian",
                    HwArchitecture = "amd64",
                    Version = new Version("1.1")
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
            
            var releaseArtifactService = new FsReleaseArtifactService(mockedRepository, Substitute.For<ILogger<FsReleaseArtifactService>>());

            //Act
            var testPlatforms1 = releaseArtifactService.GetPlatforms("product1", "1.0");
            var testPlatforms2 = releaseArtifactService.GetPlatforms("product1", "1.1");

            //Assert
            Assert.Equal(expectedPlatforms1, testPlatforms1);
            Assert.Equal(expectedPlatforms2, testPlatforms2);
        }

        [Fact]
        public void TestGetVersions()
        {
            //Setup 
            List<string> expectedVersions1 = new List<string>()
                {"1.2", "1.1"};

            List<string> expectedVersions2 = new List<string>()
            {
                "1.0"
            };
            
            var repositoryMock = new Mock<IReleaseArtifactRepository>();
            repositoryMock.Setup(r => r.GetInfosByProductName("product1")).Returns(new List<ProductInformationModel>(testProductInfos));
            var mockedRepository = repositoryMock.Object;
            
            var releaseArtifactService = new FsReleaseArtifactService(mockedRepository, Substitute.For<ILogger<FsReleaseArtifactService>>());
            
            //Act
            var testVersions1 = releaseArtifactService.GetVersions("product1", "debian", "amd64");
            var testVersions2 = releaseArtifactService.GetVersions("product1", "ubuntu", "amd64");

            //Assert
            Assert.Equal(expectedVersions1, testVersions1);
            Assert.Equal(expectedVersions2, testVersions2);
        }

        [Fact]
        public void TestGetLatestVersion()
        {
            //Setup 
            var repositoryMock = new Mock<IReleaseArtifactRepository>();
            repositoryMock.Setup(r => r.GetInfosByProductName("product1")).Returns(new List<ProductInformationModel>(testProductInfos));
            var mockedRepository = repositoryMock.Object;
            
            var releaseArtifactService = new FsReleaseArtifactService(mockedRepository, Substitute.For<ILogger<FsReleaseArtifactService>>());
            
            //Act
            var testVersions1 = releaseArtifactService.GetLatestVersion("product1", "debian", "amd64");
            var testVersions2 = releaseArtifactService.GetLatestVersion("product1", "ubuntu", "amd64");

            //Assert
            Assert.Equal("1.2", testVersions1);
            Assert.Equal("1.0", testVersions2);
        }
    }
}