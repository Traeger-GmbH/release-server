using System;
using System.IO;
using System.IO.Compression;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ReleaseArtifactMapperTest
    {
        private readonly byte[] testFile;

        public ReleaseArtifactMapperTest()
        {
            var projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
            testFile = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "test_zip.zip"));
        }
        
        [Fact]
        public void ConvertToReleaseArtifactTest()
        {
            //Setup
            using var stream = new MemoryStream(testFile);
            var testZip = new ZipArchive(stream);
            
            var expectedArtifact = new ReleaseArtifactModel
            {
                ProductInformation = new ProductInformationModel
                {
                    ProductIdentifier = "product",
                    Version = "1.1".ToProductVersion(),
                    Os = "ubuntu",
                    HwArchitecture = "amd64"
                },
                Payload = testZip
            };
            
            var testArtifact = ReleaseArtifactMapper.ConvertToReleaseArtifact("product",  "ubuntu",
                "amd64", "1.1", testZip);
            
            testArtifact.Should().BeEquivalentTo(expectedArtifact);
        }
    }
}