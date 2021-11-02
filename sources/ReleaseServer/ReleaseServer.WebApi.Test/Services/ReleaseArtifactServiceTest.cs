//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactServiceTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using NSubstitute;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ReleaseArtifactServiceTest
    {
        private IReleaseArtifactService fsReleaseArtifactService;
        private IReleaseArtifactRepository fsReleaseArtifactRepository;
        private readonly string projectDirectory;

        public ReleaseArtifactServiceTest()
        {
            //Setup
            //Could be done smarter!
            projectDirectory = TestUtils.GetProjectDirectory();

            var artifactRootDirectory = new DirectoryInfo(Path.Combine(projectDirectory, "TestData"));
            var backupRootDirectory = new DirectoryInfo(Path.Combine(projectDirectory, "TestBackupDir"));

            fsReleaseArtifactRepository = new FsReleaseArtifactRepository(
                    Substitute.For<ILogger<FsReleaseArtifactRepository>>(),
                    artifactRootDirectory,
                    backupRootDirectory
                );
            fsReleaseArtifactService = new FsReleaseArtifactService(
                    fsReleaseArtifactRepository,
                    Substitute.For<ILogger<FsReleaseArtifactService>>()
                );
        }

        [Fact]
        public async void TestGetLatestVersion()
        {
           //Act
            var testVersions1 = await fsReleaseArtifactService.GetLatestVersion("productx", "debian", "amd64");
            var testVersions2 = await fsReleaseArtifactService.GetLatestVersion("productx", "ubuntu", "amd64");

            //Assert
            Assert.Equal(new ProductVersion("1.2-beta"), testVersions1);
            Assert.Equal(new ProductVersion("1.1"), testVersions2);
        }
        
        [Fact]
        public async void TestGetLatestVersion_Not_Found()
        {
            //Act
            var testVersion = await fsReleaseArtifactService.GetLatestVersion("nonExistentProduct", "noOs", "noArch");

            //Assert
            Assert.Null(testVersion);
        }

        [Fact]
        public void TestValidateUploadPayload_Valid()
        {
            //Prepare
            var testUploadPayload = File.ReadAllBytes(Path.Combine(projectDirectory, "TestData", "validateUploadPayload",
                "valid", "test_payload_valid.zip")); 
            
            var testFormFile = new FormFile(new MemoryStream(testUploadPayload),
                baseStreamOffset: 0,
                length: testUploadPayload.Length,
                name: "test data",
                fileName: "test_zip_valid.zip");
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.True(validationResult.IsValid);
            Assert.Null(validationResult.ValidationErrors);
        }

        [Fact]
        public void TestValidateUploadPayload_Invalid_NoArtifactFile()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "releaseNotes.json"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "deployment.json")
            });

            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_without_artifact.zip");

            var expectedValidationError = "the expected artifact \"testprogram.exe\" does not exist in the uploaded payload!";
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_NoReleaseNotes()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "deployment.json")
            });
            
            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_without_release_notes.zip");

            var expectedValidationError = "the expected release notes file \"releaseNotes.json\" does not exist in the uploaded payload!";
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_MetaJsonFormat()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "invalid_meta_format", "deployment.json"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "releaseNotes.json"),
                
            });

            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_invalid_meta_format.zip");

            var expectedValidationError = "the deployment meta information (deployment.json) is invalid! " +
                "Error: Unexpected character encountered while parsing value: i. Path '', line 0, position 0.";
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_Meta_Structure()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "invalid_meta_structure", "deployment.json"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "releaseNotes.json"),
                
            });
            
            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_invalid_meta_structure.zip");

            var expectedValidationError = "the deployment meta information (deployment.json) is invalid! Error: " + 
                                          "Required property 'ReleaseNotesFileName' not found in JSON. Path '', line 3, position 1.";

            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_NoMeta()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "releaseNotes.json")
            });
            
            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_without_meta.zip");

            var expectedValidationError = "the deployment.json does not exist in the uploaded payload!";
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_ReleaseNotes_JsonFormat()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "deployment.json"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "test_payload_invalid_release_notes_format", "releaseNotes.json"),
                
            });
            
            var testZipFile = TestUtils.CreateTestZipFile(filePaths);

            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_invalid_release_notes_format.zip");

            var expectedValidationError = "the release notes file \"releaseNotes.json\" is an invalid json file! " +
                                          "Error: Unexpected character encountered while parsing value: i. Path '', line 0, position 0.";
            
            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
        
        [Fact]
        public void TestValidateUploadPayload_Invalid_ReleaseNotes_Structure()
        {
            //Prepare
            var filePaths = new List<string>(new[]
            {
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "testprogram.exe"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "valid", "deployment.json"),
                Path.Combine(projectDirectory, "TestData", "validateUploadPayload", "test_payload_invalid_release_notes_structure", "releaseNotes.json"),
                
            });
            
            var testZipFile = TestUtils.CreateTestZipFile(filePaths);
            
            var testFormFile = new FormFile(new MemoryStream(testZipFile),
                baseStreamOffset: 0,
                length: testZipFile.Length,
                name: "test data",
                fileName: "test_payload_invalid_release_notes_structure.zip");

            var expectedValidationError = "the release notes file \"releaseNotes.json\" is an invalid json file!" +
                                          " Error: Required property 'Changes' not found in JSON. Path '', line 4, position 1.";

            //Act
            var validationResult = fsReleaseArtifactService.ValidateUploadPayload(testFormFile);

            Assert.False(validationResult.IsValid);
            Assert.Equal(expectedValidationError, validationResult.ValidationErrors.First());
        }
    }
}