//--------------------------------------------------------------------------------------------------
// <copyright file="SwaggerDocumentationTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// The Swagger documentation is public and meant for external users: every operation is described,
    /// authenticated operations are marked and the published docs/api/swagger.json is up to date.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class SwaggerDocumentationTest : IDisposable
    {
        private static readonly string[] PublicOperations =
        {
            "GET /artifacts",
            "GET /artifacts/{product}/info",
            "GET /artifacts/{product}/{os}/{architecture}/latest",
            "GET /artifacts/{product}/{os}/{architecture}/{version}",
        };

        private readonly ReleaseServerTestHost host = new ReleaseServerTestHost();

        public void Dispose()
        {
            host.Dispose();
        }

        [Fact]
        public async Task Operations_AreDescribedAndAuthenticationIsMarked()
        {
            var document = await LoadDocument();

            foreach (var (name, operation) in Operations(document))
            {
                Assert.False(string.IsNullOrWhiteSpace((string)operation["summary"]), $"{name}: summary missing");
                var requiresAuthentication = operation["security"] != null;

                if (PublicOperations.Contains(name))
                {
                    Assert.False(requiresAuthentication, $"{name} must be public");
                    Assert.DoesNotContain("requires authentication", (string)operation["summary"]);
                }
                else
                {
                    Assert.True(requiresAuthentication, $"{name} must require authentication");
                    Assert.EndsWith("(requires authentication)", (string)operation["summary"]);
                    Assert.StartsWith("**Requires authentication**", (string)operation["description"]);
                    Assert.NotNull(operation["responses"]["401"]);
                }
            }
        }

        [Fact]
        public async Task PublicResponses_HaveExamples()
        {
            var document = await LoadDocument();

            var info = document["paths"]["/artifacts/{product}/info"]["get"]["responses"]["200"]["content"]["application/json"];
            var list = document["paths"]["/artifacts"]["get"]["responses"]["200"]["content"]["application/json"];

            Assert.Equal("example-product", (string)(info["example"] ?? info["examples"]?.First?.First?["value"])["identifier"]);
            Assert.NotNull(list["example"] ?? list["examples"]);
        }

        [Fact]
        public async Task PublishedSwaggerDocument_IsUpToDate()
        {
            // docs/api/swagger.json is linked in the README. Record it with RELEASE_SERVER_UPDATE_GOLDEN=1.
            var path = Path.Combine(TestUtils.GetProjectDirectory(), "..", "..", "..", "docs", "api", "swagger.json");
            // Line breaks inside the descriptions depend on the line endings of the checked out sources.
            var actual = (await LoadDocument()).ToString(Formatting.Indented).Replace("\r\n", "\n").Replace("\\r\\n", "\\n") + "\n";

            if (Environment.GetEnvironmentVariable("RELEASE_SERVER_UPDATE_GOLDEN") == "1")
            {
                File.WriteAllText(path, actual);
                return;
            }

            Assert.Equal(File.ReadAllText(path).Replace("\r\n", "\n"), actual);
        }

        private async Task<JObject> LoadDocument()
        {
            using var client = host.CreateClient();
            using var reader = new JsonTextReader(new StringReader(await client.GetStringAsync("/swagger/v1/swagger.json")))
            {
                DateParseHandling = DateParseHandling.None
            };
            return JObject.Load(reader);
        }

        private static (string Name, JToken Operation)[] Operations(JObject document)
        {
            return ((JObject)document["paths"]).Properties()
                .SelectMany(path => ((JObject)path.Value).Properties()
                    .Select(method => (method.Name.ToUpperInvariant() + " " + path.Name, method.Value)))
                .ToArray();
        }
    }
}
