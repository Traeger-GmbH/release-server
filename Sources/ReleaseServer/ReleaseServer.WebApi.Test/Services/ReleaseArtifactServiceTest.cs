using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ReleaseServer.WebApi.Services;
using ReleaseServer.WebApi.Repositories;
using Xunit;
using ReleaseServer.WebApi.Test.Utils;

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
            ProjectDirectory = TestUtils.GetProjectDirectory();

            var artifactRootDirectory = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestData"));
            var backupRootDirectory = new DirectoryInfo(Path.Combine(ProjectDirectory, "TestBackupDir"));

            FsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    artifactRootDirectory,
                    backupRootDirectory
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

        [Fact]
        public void TestValidateUploadPayload_Valid()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_zip.zip");
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.True(validationResult.IsValid);
            Assert.Null(validationResult.ValidationError);
        }

        [Fact]
        public void TestValidateUploadPayload_Invalid_NoArtifactFile()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload_without_artifact.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_payload_without_artifact.zip");

            var expectedValidationError = "the expected artifact \"testprogram.exe\" does not exist in the uploaded payload!";
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationError);
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_NoChangelog()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload_without_changelog.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_payload_without_changelog.zip");

            var expectedValidationError = "the expected changelog file \"changelog.txt\" does not exist in the uploaded payload!";
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationError);
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_InvalidMeta_Json_Format()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload_invalid_meta_format.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_payload_invalid_meta_format.zip");

            var expectedValidationError = "the deployment meta information (deployment.json) is invalid! " +
                                          "Error: Unexpected character encountered while parsing value:" +
                                          " i. Path '', line 0, position 0.";
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationError);
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_InvalidMeta_Structure()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload_invalid_meta_structure.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_payload_invalid_meta_structure.zip");

            var expectedValidationError = "the deployment meta information (deployment.json) is invalid!" +
                                          " Error: Required property 'ChangelogFileName' not found in JSON." +
                                          " Path '', line 3, position 1.";
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationError);
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_NoMeta()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(ProjectDirectory, "TestData", "validateUploadPayload",
                "test_payload_without_meta.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_payload_without_meta.zip");

            var expectedValidationError = "the deployment.json does not exist in the uploaded payload!";
            
            //Act
            var validationResult = FsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationError);
        }
    }
}