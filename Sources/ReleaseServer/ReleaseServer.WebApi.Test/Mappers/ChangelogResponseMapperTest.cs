using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ChangelogResponseMapperTest
    {
        [Fact]
        public void ConvertStringToChangelogResponse()
        {
            var testString = "This is a test changelog!";

            var expectedResponse = new ChangelogResponseModel
            {
                Changelog = "This is a test changelog!"
            };

            var testChangelogResponse = testString.toChangelogResponse();
            
            testChangelogResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}