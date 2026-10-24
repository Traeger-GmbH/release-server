//--------------------------------------------------------------------------------------------------
// <copyright file="LiveSnapshotTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Rebuilds the production data set from the recorded responses of https://releases.traeger.de
    /// (see LiveSnapshot/README.txt) and verifies that the server answers exactly like production.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class LiveSnapshotTest : IDisposable
    {
        private static readonly string SnapshotDirectory =
            Path.Combine(TestUtils.GetProjectDirectory(), "Integration", "LiveSnapshot");

        private readonly ReleaseServerTestHost host;

        public LiveSnapshotTest()
        {
            host = new ReleaseServerTestHost();

            foreach (var file in Directory.GetFiles(Path.Combine(SnapshotDirectory, "info"), "*.json"))
            {
                CreateStoreFromInfo(Path.GetFileNameWithoutExtension(file), File.ReadAllText(file, Encoding.UTF8));
            }
        }

        public void Dispose()
        {
            host.Dispose();
        }

        public static IEnumerable<object[]> Products()
        {
            var products = (JArray)ReadJson(File.ReadAllText(Path.Combine(SnapshotDirectory, "products.json")))["products"];
            return products.Select(product => new object[] { (string)product });
        }

        [Fact]
        public async Task ProductList_MatchesProduction()
        {
            using var client = host.CreateClient();

            var response = await client.GetAsync("/artifacts/");

            Assert.Equal(
                Golden.NormalizeJson(File.ReadAllText(Path.Combine(SnapshotDirectory, "products.json"))),
                Golden.NormalizeJson(await response.Content.ReadAsStringAsync()));
        }

        [Theory]
        [MemberData(nameof(Products))]
        public async Task ProductInfo_MatchesProduction(string product)
        {
            using var client = host.CreateClient();
            var expected = File.ReadAllText(Path.Combine(SnapshotDirectory, "info", product + ".json"), Encoding.UTF8);

            var response = await client.GetAsync($"/artifacts/{Uri.EscapeDataString(product)}/info?limit=1000");
            var actual = await response.Content.ReadAsStringAsync();

            var expectedStatus = ReadJson(expected)["status"]?.Value<int>() ?? 200;
            Assert.Equal(expectedStatus, (int)response.StatusCode);
            Assert.Equal(Golden.NormalizeJson(expected), Golden.NormalizeJson(actual));

            // The platform order depends on the directory enumeration. If every release has at most one
            // platform, the raw body must match byte by byte (escaping, number and date formats).
            if (response.StatusCode == HttpStatusCode.OK && ReadJson(expected)["releases"].All(release => release["platforms"].Count() <= 1))
            {
                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public async Task SwaggerOperations_MatchProduction()
        {
            using var client = host.CreateClient();

            var response = await client.GetAsync("/swagger/v1/swagger.json");

            Assert.Equal(
                Operations(File.ReadAllText(Path.Combine(SnapshotDirectory, "swagger.json"))),
                Operations(await response.Content.ReadAsStringAsync()));
        }

        private void CreateStoreFromInfo(string product, string infoJson)
        {
            var info = ReadJson(infoJson);
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, product));

            if (info["releases"] == null)
            {
                // Products without releases only exist as an empty directory.
                return;
            }

            foreach (var release in info["releases"])
            {
                var releaseNotes = new JObject
                {
                    ["ReleaseDate"] = release["releaseDate"],
                    ["IsPreviewRelease"] = release["isPreviewRelease"],
                    ["IsSecurityPatch"] = release["isSecurityPatch"],
                    ["Changes"] = release["changes"],
                };

                foreach (var platform in release["platforms"].Select(token => (string)token))
                {
                    var parts = platform.Split('/');
                    LegacyStore.AddRelease(host.ArtifactDirectory, product, parts[0], parts[1], (string)release["version"],
                        "artifact.bin", releaseNotes.ToString(Formatting.Indented), pascalCase: true);
                }
            }
        }

        private static List<string> Operations(string swaggerJson)
        {
            var document = JObject.Parse(swaggerJson);
            return ((JObject)document["paths"]).Properties()
                .SelectMany(path => ((JObject)path.Value).Properties().Select(method => method.Name.ToUpperInvariant() + " " + path.Name))
                .OrderBy(operation => operation, StringComparer.Ordinal)
                .ToList();
        }

        private static JToken ReadJson(string json)
        {
            using var reader = new JsonTextReader(new StringReader(json)) { DateParseHandling = DateParseHandling.None };
            return JToken.ReadFrom(reader);
        }
    }
}
