using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReleaseServer.WebApi.Services;
using ReleaseServer.WebApi.Repositories;
using Xunit;
using Microsoft.Extensions.Configuration;
using Moq;

namespace ReleaseServer.WebApi.Test.TestData
{
    public class ReleaseArtifactServiceTest
    {
        private IReleaseArtifactService FsReleaseArtifactService;
        private IReleaseArtifactRepository FsReleaseArtifactRepository;
        private readonly string ProjectDirectory;

        public ReleaseArtifactServiceTest()
        {
            //Setup
            //Could be done smarter!
            ProjectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            var configuration = new Mock<IConfiguration>();
            configuration
                .SetupGet(x => x[It.Is<string>(s => s == "ArtifactRootDirectory")])
                .Returns(Path.Combine(ProjectDirectory, "TestData"));

            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    configuration.Object
                );
            FsReleaseArtifactService = new FsReleaseArtifactService(
                    FsReleaseArtifactRepository,
                    Substitute.For<ILogger<FsReleaseArtifactService>>()
                );
        }

        [Fact]
        public async void TestGetLatestVersion()
        {
           //Act
            var testVersions1 = await FsReleaseArtifactService.GetLatestVersion("productx", "debian", "amd64");
            var testVersions2 = await FsReleaseArtifactService.GetLatestVersion("productx", "ubuntu", "amd64");

            //Assert
            Assert.Equal("1.2-beta", testVersions1);
            Assert.Equal("1.1", testVersions2);
        }
        
        [Fact]
        public async void TestGetLatestVersion_Not_Found()
        {
            //Act
            var testVersion = await FsReleaseArtifactService.GetLatestVersion("nonExistentProduct", "noOs", "noArch");

            //Assert
            Assert.Null(testVersion);
        }
    }
}