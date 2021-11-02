namespace ReleaseServer.WebApi.Test.Models
{
    using FluentAssertions;
    using ReleaseServer.WebApi.Models;
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class ReleaseNotesTest
    {
        [Fact]
        public void Parse_FromString_Works()
        {
            var jsonString = "{" +
                "\"version\": \"2.0.0-preview-2\"," +
                "\"releaseDate\": \"2021-10-28\"," +
                "\"isPreviewRelease\": true," + 
                "\"IsSecurityPatch\": false," +
                " \"changes\": {" +
                    "\"de\": [{" +
                        "\"platforms\": null," +
                        "\"added\": []," +
                        "\"fixed\": []," +
                        "\"breaking\": []," +
                        "\"deprecated\": []" +
                    "}]," +
                    "\"en\": [{" +
                        "\"platforms\": null," +
                        "\"added\": []," +
                        "\"fixed\": []," +
                        "\"breaking\": []," +
                        "\"deprecated\": []" +
                    "}]" + 
                "}," +
                "\"platforms\": [" +
                    "\"windows/x64\"," +
                    "\"windows/x86\"," +
                    "\"windows/arm64\"," +
                    "\"linux/x64\"," +
                    "\"linux/arm64\"," +
                    "\"linux/arm32\"" +
                "] }";

            Action act = () => ReleaseNotes.FromString(jsonString);
            act.Should().NotThrow();
        }
    }
}
