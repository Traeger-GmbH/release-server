using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using FluentAssertions;
using ReleaseServer.WebApi.Common;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test.Common
{
    public class JsonSerializeTest
    {
        private readonly string ProjectDirectory;
        private readonly ReleaseNotes ExpectedReleaseNotes;
        private readonly DeploymentMetaInfo ExpectedMeta;

        public JsonSerializeTest()
        {
            ProjectDirectory = TestUtils.GetProjectDirectory();
            ExpectedReleaseNotes = new ReleaseNotes
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
            ExpectedMeta =  new DeploymentMetaInfo
            {
                ReleaseNotesFileName = "releaseNotes.json",
                ArtifactFileName = "artifact.zip",
                ReleaseDate = new DateTime(2020, 02, 01)
            };
        }
        
        
        [Fact]
        public void DeserializeReleaseNotes()
        {
            var serializer = new JsonSerializable<ReleaseNotes>();

            var parsedReleaseNotes = serializer.FromJsonFile(Path.Combine(ProjectDirectory, "TestData", "testReleaseNotes.json"));

            parsedReleaseNotes.Should().BeEquivalentTo(ExpectedReleaseNotes);
        }

        [Fact]
        public void DeserializeDeploymentMetaInfo()
        {
            var deserializer = new JsonSerializable<DeploymentMetaInfo>();

            var parsedMeta = deserializer.FromJsonFile(Path.Combine(ProjectDirectory, "TestData", "testDeployment.json"));
            
            parsedMeta.Should().BeEquivalentTo(ExpectedMeta);
        }
    }
}