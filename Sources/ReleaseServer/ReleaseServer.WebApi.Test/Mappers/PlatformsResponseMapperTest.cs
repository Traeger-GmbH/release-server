using System.Collections.Generic;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class PlatformsResponseMapperTest
    {
        [Fact]
        public void ConvertListOfStringsToPlatformsResponse()
        {

            var testPlatformsList = new List<string> {"debian-arm64", "debian-amd64", "ubuntu-amd64"};

            var expectedPlatformsResponse = new PlatformsResponse
            {
                Platforms = new List<string> {"debian-arm64", "debian-amd64", "ubuntu-amd64"}
            };

            var testPlatformsResponse = testPlatformsList.ToPlatformsResponse();
            
            testPlatformsResponse.Should().BeEquivalentTo(expectedPlatformsResponse);
        }
    }
}