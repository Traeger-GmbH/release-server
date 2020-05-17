using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ReleaseInformationResponseMapperTest
    {
        [Fact]
        public void TestToReleaseInformationResponse()
        {
            //Setup
            var testReleaseInformationModel = new ReleaseInformationModel
            {
                ReleaseNotes = new ReleaseNotesModel
                {
                    ReleaseNotesSet = new Dictionary<CultureInfo, List<ChangeSet>>
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
                        }
                    }
                },
                ReleaseDate = new DateTime(2020, 02, 01)
            };

            var expectedReleaseInformationResponse = new ReleaseInformationResponseModel
            {
                ReleaseNotes = new Dictionary<string, List<ChangeSet>>
                {
                    {
                        "de", new List<ChangeSet>
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
                    }
                },
                ReleaseDate = new DateTime(2020, 02, 01)
            };
            
            //Act
            var testResponse = testReleaseInformationModel.ToReleaseInformationResponse();
            
            //Assert
            testResponse.Should().BeEquivalentTo(expectedReleaseInformationResponse);
        }
    }
}