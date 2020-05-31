using System;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test.Extensions
{
    public class DeploymentMetaInfoExtensiontest
    {
        [Fact]
        public void ValidateDeploymentMeta_Valid()
        {
            var testDeploymentMetaInfo = new DeploymentMetaInfo
            {
                ArtifactFileName = "testArtifact.zip",
                ChangelogFileName = "changelog.txt",
                ReleaseDate = new DateTime(2020, 02, 01)
            };

            Assert.True(testDeploymentMetaInfo.IsValid());
        }
        
        [Fact]
        public void ValidateDeploymentMeta_Invalid()
        {
            
            //Without Changelog
            var testDeploymentMetaInfo1 = new DeploymentMetaInfo
            {
                ArtifactFileName = "testArtifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
            
            //Without ReleaseDate
            var testDeploymentMetaInfo2 = new DeploymentMetaInfo
            {
                ArtifactFileName = "testArtifact.zip",
                ChangelogFileName = "changelog.txt",
            };
            
            //Without ArtifactFileName
            var testDeploymentMetaInfo3 = new DeploymentMetaInfo
            {
                ChangelogFileName = "changelog.txt",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
            
            //Empty
            var testDeploymentMetaInfo4 = new DeploymentMetaInfo();
            
            Assert.False(testDeploymentMetaInfo1.IsValid());
            Assert.False(testDeploymentMetaInfo2.IsValid());
            Assert.False(testDeploymentMetaInfo3.IsValid());
            Assert.False(testDeploymentMetaInfo4.IsValid());
        }
    }
}