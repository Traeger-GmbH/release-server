using System;
using System.IO;
using FluentAssertions;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class DeploymentMetaMapperTest
    {
        private readonly string ProjectDirectory;
        private readonly DeploymentMetaInfo ExpectedMeta;

        public DeploymentMetaMapperTest()
        {
            ProjectDirectory = TestUtils.GetProjectDirectory();
            ExpectedMeta =  new DeploymentMetaInfo
            {
                ChangelogFileName = "changelog.txt",
                ArtifactFileName = "artifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
        

        [Fact]
        public void ConvertJsonToDeploymentMetaInfo_String()
        {
            var parsedMeta = DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(Path.Combine(ProjectDirectory, "TestData","testDeployment.json"));

            parsedMeta.Should().BeEquivalentTo(ExpectedMeta);
        }
        
        [Fact]
        public void ConvertJsonToDeploymentMetaInfo_ByteArray()
        {
            var testMetaInfoByteArray = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "testDeployment.json"));
            
            var parsedMeta = DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(testMetaInfoByteArray);

            parsedMeta.Should().BeEquivalentTo(ExpectedMeta);
        }
    }
}