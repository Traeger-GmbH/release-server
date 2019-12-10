using System;
using System.IO;
using Microsoft.Extensions.Logging;
using NSubstitute;
using release_server_web_api.Services;
using ReleaseServer.WebApi.Repositories;
using Xunit;

namespace release_server_web_api_test.TestData
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
            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(Substitute.For<ILogger<FsReleaseArtifactRepository>>(),Path.Combine(ProjectDirectory, "TestData"));
            FsReleaseArtifactService = new FsReleaseArtifactService(FsReleaseArtifactRepository, Substitute.For<ILogger<FsReleaseArtifactService>>());
        }

        [Fact]
        public void TestGetLatestVersion()
        {
           //Act
            var testVersions1 = FsReleaseArtifactService.GetLatestVersion("productx", "debian", "amd64");
            var testVersions2 = FsReleaseArtifactService.GetLatestVersion("productx", "ubuntu", "amd64");

            //Assert
            Assert.Equal("1.2-beta", testVersions1);
            Assert.Equal("1.1", testVersions2);
        }
    }
}