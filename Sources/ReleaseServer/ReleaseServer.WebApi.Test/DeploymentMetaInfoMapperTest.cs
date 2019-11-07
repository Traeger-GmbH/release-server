using System;
using System.IO;
using FluentAssertions;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Mappers;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class DeploymentMetaMapperTest
    {

        [Fact]
        public void ConvertJsonToDeploymentMetaInfo()
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;

            var expectedMeta = new DeploymentMetaInfo
            {
                ChangelogFileName = "changelog.txt",
                ArtifactFileName = "artifact.zip",
            };

            var parsedMeta = DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(Path.Combine(projectDirectory, "TestData","testDeployment.json"));

            parsedMeta.Should().BeEquivalentTo(expectedMeta);
        }
    }
}