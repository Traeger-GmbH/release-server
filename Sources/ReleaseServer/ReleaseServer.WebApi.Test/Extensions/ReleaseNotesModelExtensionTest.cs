using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using ReleaseServer.WebApi.Extensions;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test.Extensions
{
    public class ReleaseNotesModelExtensionTest
    {
        [Fact]
        public void TestGetReleaseNotesResponse()
        {
            //Setup
            var testReleaseNotesModel = new ReleaseNotes
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
            };

            var expectedResponse = new Dictionary<string, List<ChangeSet>>
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
            };

            //Act
            var testResponse = testReleaseNotesModel.GetReleaseNotesResponse();

            //Assert
            testResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
