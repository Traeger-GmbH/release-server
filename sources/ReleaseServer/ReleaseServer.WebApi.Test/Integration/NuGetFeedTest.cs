//--------------------------------------------------------------------------------------------------
// <copyright file="NuGetFeedTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Versioning;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Tests the read-only NuGet v3 feed: golden files of the feed resources and a real NuGet client
    /// ("dotnet add package", "dotnet package search") against a running server.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class NuGetFeedTest : IDisposable
    {
        private readonly List<ReleaseServerTestHost> hosts = new List<ReleaseServerTestHost>();

        public void Dispose()
        {
            hosts.ForEach(host => host.Dispose());
        }

        /// <summary>
        /// Creates a valid .nupkg with the NuGet package builder.
        /// </summary>
        public static byte[] CreatePackage(string id, string version, params (string Id, string Range)[] dependencies)
        {
            var builder = new PackageBuilder
            {
                Id = id,
                Version = NuGetVersion.Parse(version),
                Description = $"Test package {id}",
                Title = id + " title",
                Copyright = "Test",
            };
            builder.Authors.Add("Traeger Test");
            builder.Tags.Add("opc");
            builder.Tags.Add("test");
            builder.DependencyGroups.Add(new PackageDependencyGroup(NuGetFramework.Parse("netstandard2.0"),
                dependencies.Select(dependency => new PackageDependency(dependency.Id, VersionRange.Parse(dependency.Range)))));
            builder.Files.Add(new InMemoryPackageFile("lib/netstandard2.0/_._", Array.Empty<byte>()));

            using var stream = new MemoryStream();
            builder.Save(stream);
            return stream.ToArray();
        }

        private static async Task Upload(HttpClient client, string product, string version, string fileName, byte[] package, string releaseDate)
        {
            var releaseNotes = $"{{ \"releaseDate\": \"{releaseDate}\", \"changes\": {{ \"en\": [ {{ \"added\": [\"Release {version}\"] }} ] }} }}";
            var payload = Zip.CreateArtifactPayload(fileName, package, releaseNotes);
            var response = await client.PutAsync($"/artifacts/{product}/any/any/{version}", Zip.CreateFormContent("artifact", payload, "payload.zip"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Products named like the OPC UA SDK products, the package ID comes from the .nuspec.
        /// </summary>
        private static async Task UploadTestPackages(HttpClient client)
        {
            await Upload(client, "Test.Dependency.nupkg", "2.0.0", "test.dependency.2.0.0.nupkg", CreatePackage("Test.Dependency", "2.0.0"), "2026-01-10");
            await Upload(client, "Opc.UaFx.Advanced.nupkg", "2.56.0", "opc.uafx.advanced.2.56.0.2026-08-19-2.56.0.0.nupkg",
                CreatePackage("Opc.UaFx.Advanced", "2.56.0", ("Test.Dependency", "[2.0.0, )")), "2026-08-19");
            await Upload(client, "Opc.UaFx.Advanced.nupkg", "2.57.1", "opc.uafx.advanced.2.57.1.2026-10-23-2.57.1.0.nupkg",
                CreatePackage("Opc.UaFx.Advanced", "2.57.1", ("Test.Dependency", "[2.0.0, )")), "2026-10-23");
            await Upload(client, "Opc.UaFx.Advanced.nupkg", "2.58.0-preview.1", "opc.uafx.advanced.2.58.0-preview.1.nupkg",
                CreatePackage("Opc.UaFx.Advanced", "2.58.0-preview.1"), "2026-11-02");
            await Upload(client, "Opc.UaFx.Advanced.nupkg", "2.0.1.1", "opc.uafx.advanced.2.0.1.1.2017-06-07-2.0.1.1.nupkg",
                CreatePackage("Opc.UaFx.Advanced", "2.0.1.1"), "2017-06-07");
            // Not every .zip is a package; an artifact that is no NuGet package is ignored by the feed
            await Upload(client, "Opc.UaFx.Advanced", "2.57.1", "opc.uafx.advanced.2026-10-23-2.57.1.0.zip", Encoding.UTF8.GetBytes("zip"), "2026-10-23");
        }

        private async Task<(ReleaseServerTestHost Host, HttpClient Client)> CreateFeed(bool useKestrel = false)
        {
            var host = new ReleaseServerTestHost(useKestrel: useKestrel);
            hosts.Add(host);
            var client = host.CreateAuthenticatedClient();
            await UploadTestPackages(client);
            return (host, client);
        }

        [Theory]
        [InlineData("nuget-index", "/nuget/v3/index.json")]
        [InlineData("nuget-flatcontainer-versions", "/nuget/v3/flatcontainer/opc.uafx.advanced/index.json")]
        [InlineData("nuget-flatcontainer-versions-mixed-case", "/nuget/v3/flatcontainer/Opc.UaFx.Advanced/index.json")]
        [InlineData("nuget-registration-index", "/nuget/v3/registration/opc.uafx.advanced/index.json")]
        [InlineData("nuget-registration-leaf", "/nuget/v3/registration/opc.uafx.advanced/2.57.1.json")]
        [InlineData("nuget-search-release", "/nuget/v3/search?q=opc&take=20")]
        [InlineData("nuget-search-prerelease", "/nuget/v3/search?q=opc&take=20&prerelease=true&semVerLevel=2.0.0")]
        [InlineData("nuget-search-all", "/nuget/v3/search")]
        [InlineData("nuget-search-no-match", "/nuget/v3/search?q=unknown")]
        public async Task FeedResources_MatchGoldenFiles(string goldenName, string url)
        {
            var (_, client) = await CreateFeed();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task PackageContent_IsTheUploadedFile()
        {
            var (host, client) = await CreateFeed();

            var nupkg = await client.GetAsync("/nuget/v3/flatcontainer/opc.uafx.advanced/2.57.1/opc.uafx.advanced.2.57.1.nupkg");
            var nuspec = await client.GetAsync("/nuget/v3/flatcontainer/opc.uafx.advanced/2.57.1/opc.uafx.advanced.nuspec");

            Assert.Equal(HttpStatusCode.OK, nupkg.StatusCode);
            var stored = File.ReadAllBytes(Path.Combine(host.ArtifactDirectory, "Opc.UaFx.Advanced.nupkg", "any", "any", "2.57.1", "opc.uafx.advanced.2.57.1.2026-10-23-2.57.1.0.nupkg"));
            Assert.Equal(stored, await nupkg.Content.ReadAsByteArrayAsync());
            Assert.Equal(HttpStatusCode.OK, nuspec.StatusCode);
            Assert.Contains("<id>Opc.UaFx.Advanced</id>", await nuspec.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/nuget/v3/flatcontainer/unknown/index.json")]
        [InlineData("/nuget/v3/flatcontainer/opc.uafx.advanced/9.9.9/opc.uafx.advanced.9.9.9.nupkg")]
        [InlineData("/nuget/v3/flatcontainer/opc.uafx.advanced/2.57.1/other.2.57.1.nupkg")]
        [InlineData("/nuget/v3/flatcontainer/opc.uafx.advanced/2.57.1/..%2F..%2Fdeployment.json")]
        [InlineData("/nuget/v3/registration/unknown/index.json")]
        [InlineData("/nuget/v3/registration/opc.uafx.advanced/not-a-version.json")]
        public async Task UnknownResources_AreNotFound(string url)
        {
            var (_, client) = await CreateFeed();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Urls_UseTheForwardedAddressBehindAReverseProxy()
        {
            var (_, client) = await CreateFeed();
            using var request = new HttpRequestMessage(HttpMethod.Get, "/nuget/v3/index.json");
            request.Headers.Add("X-Forwarded-Proto", "https");
            request.Headers.Add("X-Forwarded-Host", "releases.example.com");

            var index = JObject.Parse(await (await client.SendAsync(request)).Content.ReadAsStringAsync());

            Assert.All(index["resources"], resource => Assert.StartsWith("https://releases.example.com/nuget/v3/", (string)resource["@id"]));
        }

        [Fact]
        public async Task Feed_RequiresNoAuthentication_AndListsNoOtherArtifacts()
        {
            var (host, _) = await CreateFeed();
            using var anonymous = host.CreateClient();

            var search = JObject.Parse(await anonymous.GetStringAsync("/nuget/v3/search?prerelease=true&semVerLevel=2.0.0"));

            Assert.Equal(new[] { "Opc.UaFx.Advanced", "Test.Dependency" }, search["data"].Select(package => (string)package["id"]));
        }

        [Fact]
        public async Task ExistingApi_IsUnchangedForPackageProducts()
        {
            // Product IDs ending with ".nupkg" are normal product names for the existing API
            var (_, client) = await CreateFeed();

            var info = await client.GetAsync("/artifacts/Opc.UaFx.Advanced.nupkg/info");
            var latest = await client.GetAsync("/artifacts/Opc.UaFx.Advanced.nupkg/any/any/latest");
            var products = JObject.Parse(await client.GetStringAsync("/artifacts/"));

            Assert.Equal("application/json; charset=utf-8", info.Content.Headers.ContentType.ToString());
            Assert.Equal(HttpStatusCode.OK, latest.StatusCode);
            // "latest" includes pre-releases (unchanged behavior of the download API)
            Assert.Equal("attachment; filename=opc.uafx.advanced.2.58.0-preview.1.nupkg", latest.Content.Headers.ContentDisposition.ToString());
            Assert.Contains("Opc.UaFx.Advanced.nupkg", products["products"].Select(product => (string)product));
        }

        [Fact]
        public async Task NuGetClient_InstallsPackageWithDependency_AndSearches()
        {
            var (host, _) = await CreateFeed(useKestrel: true);
            var feed = $"{host.ServerAddress}/nuget/v3/index.json";
            var work = Path.Combine(host.RootDirectory, "client");
            Directory.CreateDirectory(work);
            File.WriteAllText(Path.Combine(work, "Consumer.csproj"),
                "<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>");
            // Only this feed is used (no nuget.org); HTTP is allowed for the local test server
            File.WriteAllText(Path.Combine(work, "nuget.config"), $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""release-server"" value=""{feed}"" allowInsecureConnections=""true"" />
  </packageSources>
</configuration>");
            var environment = new Dictionary<string, string>
            {
                ["NUGET_PACKAGES"] = Path.Combine(host.RootDirectory, "nuget-packages"),
                ["NUGET_HTTP_CACHE_PATH"] = Path.Combine(host.RootDirectory, "nuget-http-cache"),
                ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
                ["DOTNET_NOLOGO"] = "1",
            };

            var add = await RunDotnet(work, environment, "add", "Consumer.csproj", "package", "Opc.UaFx.Advanced", "--version", "2.57.1");
            Assert.True(add.ExitCode == 0, add.Output);

            var assets = JObject.Parse(File.ReadAllText(Path.Combine(work, "obj", "project.assets.json")));
            var libraries = ((JObject)assets["libraries"]).Properties().Select(property => property.Name).ToList();
            Assert.Contains("Opc.UaFx.Advanced/2.57.1", libraries);
            Assert.Contains("Test.Dependency/2.0.0", libraries);

            var search = await RunDotnet(work, environment, "package", "search", "Opc.UaFx", "--source", feed, "--format", "json", "--prerelease");
            Assert.True(search.ExitCode == 0, search.Output);
            Assert.Contains("2.58.0-preview.1", search.Output);

            var searchWithoutPrerelease = await RunDotnet(work, environment, "package", "search", "Opc.UaFx", "--source", feed, "--format", "json");
            Assert.True(searchWithoutPrerelease.ExitCode == 0, searchWithoutPrerelease.Output);
            Assert.Contains("2.57.1", searchWithoutPrerelease.Output);
            Assert.DoesNotContain("2.58.0-preview.1", searchWithoutPrerelease.Output);
        }

        private static async Task<(int ExitCode, string Output)> RunDotnet(string workingDirectory, IDictionary<string, string> environment, params string[] arguments)
        {
            var startInfo = new ProcessStartInfo("dotnet")
            {
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var argument in arguments)
            {
                startInfo.ArgumentList.Add(argument);
            }
            foreach (var (key, value) in environment)
            {
                startInfo.Environment[key] = value;
            }

            using var process = Process.Start(startInfo);
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            return (process.ExitCode, await output + await error);
        }

        /// <summary>
        /// A package file with content from memory.
        /// </summary>
        private sealed class InMemoryPackageFile : IPackageFile
        {
            private readonly byte[] content;

            public InMemoryPackageFile(string path, byte[] content)
            {
                Path = path;
                this.content = content;
            }

            public string Path { get; }

            public string EffectivePath => Path;

            public System.Runtime.Versioning.FrameworkName TargetFramework => null;

            public NuGetFramework NuGetFramework => NuGetFramework.Parse("netstandard2.0");

            public DateTimeOffset LastWriteTime => new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            public Stream GetStream() => new MemoryStream(content);
        }
    }
}
