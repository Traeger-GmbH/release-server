//--------------------------------------------------------------------------------------------------
// <copyright file="JsonSerializeTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

using FluentAssertions;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test.Common
{
    public class JsonSerializeTest
    {
        private readonly string projectDirectory;
        private readonly ReleaseNotes expectedReleaseNotes;
        private readonly DeploymentMetaInfo expectedMeta;

        public JsonSerializeTest()
        {
            projectDirectory = TestUtils.GetProjectDirectory();
            expectedReleaseNotes = new ReleaseNotes
            {
                Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                {
                    {
                        new CultureInfo("de"), new List<ChangeSet>
                        {
                            new ChangeSet
                            {
                                Platforms = new List<string> {"windows/any", "linux/rpi"},
                                Added = new List<string> {"added de 1", "added de 2"},
                                Fixed = new List<string> {"fix de 1", "fix de 2"},
                                Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                            }
                        }
                    },
                    {
                        new CultureInfo("en"), new List<ChangeSet>
                        {
                            new ChangeSet
                            {
                                Platforms = new List<string> {"windows/any", "linux/rpi"},
                                Added = new List<string> {"added en 1", "added en 2"},
                                Fixed = new List<string> {"fix en 1", "fix en 2"},
                                Breaking = new List<string> {"breaking en 1", "breaking en 2"},
                                Deprecated = new List<string> {"deprecated en 1", "deprecated en 2"}
                            },
                            new ChangeSet
                            {
                                Platforms = null,
                                Added = new List<string> {"added en 3"},
                                Fixed = new List<string> {"fix en 3"},
                                Breaking = new List<string> {"breaking en 3"},
                                Deprecated = new List<string> {"deprecated en 3"}
                            }
                        }
                    }
                }
            };
            expectedMeta =  new DeploymentMetaInfo
            {
                ReleaseNotesFileName = "releaseNotes.json",
                ArtifactFileName = "artifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
        
        
        [Fact]
        public void DeserializeReleaseNotes()
        {
            var parsedReleaseNotes = ReleaseNotes.FromJsonFile(Path.Combine(projectDirectory, "TestData", "testReleaseNotes.json"));

            parsedReleaseNotes.Should().BeEquivalentTo(expectedReleaseNotes);
        }

        [Fact]
        public void DeserializeDeploymentMetaInfo_Success()
        {
            var parsedMeta = DeploymentMetaInfo.FromJsonFile(Path.Combine(projectDirectory, "TestData", "testDeployment.json"));
            
            parsedMeta.Should().BeEquivalentTo(expectedMeta);
        }
        
        [Fact]
        public void DeserializeDeploymentMetaInfo_Error_No_ReleaseDate()
        {
            var pathToInvalidMeta = Path.Combine(projectDirectory, "TestData", "validateUploadPayload",
                "invalid_meta_format", "no_release_date_deployment.json");
            
            var exception = Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => 
                DeploymentMetaInfo.FromJsonFile(pathToInvalidMeta));

            Assert.Equal("Required property 'ReleaseDate' not found in JSON. Path '', line 4, position 1.",
                exception.Message);
        }
        
        [Fact]
        public void DeserializeDeploymentMetaInfo_Error_Empty_ReleaseDate()
        {
            var pathToMetaWithEmptyDate = Path.Combine(projectDirectory, "TestData", "validateUploadPayload",
                "invalid_meta_format", "empty_release_date_deployment.json");
            
            var exception = Assert.Throws<Newtonsoft.Json.JsonSerializationException>(() => 
                DeploymentMetaInfo.FromJsonFile(pathToMetaWithEmptyDate));
            
            Assert.Equal("Error converting value {null} to type 'System.DateTime'. Path 'ReleaseDate', line 4, position 19.",
                exception.Message);
        }
    }
}