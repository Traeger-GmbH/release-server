//--------------------------------------------------------------------------------------------------
// <copyright file="StorageHardeningTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Tests that uploads cannot write outside of the artifact directory, that requests cannot read or
    /// delete outside of it, and that broken or internal directories do not affect the public API.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class StorageHardeningTest : IDisposable
    {
        private readonly ReleaseServerTestHost host;

        public StorageHardeningTest()
        {
            host = new ReleaseServerTestHost();
            LegacyStore.Create(host.ArtifactDirectory);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        [Theory]
        [InlineData("../evil.txt")]
        [InlineData("..\\evil.txt")]
        [InlineData("sub/evil.txt")]
        [InlineData("{root}/evil.txt")]
        [InlineData("C:\\evil.txt")]
        [InlineData("..")]
        [InlineData("deployment.json")]
        public async Task UploadArtifact_WithUnsafeArtifactFileName_IsRejected(string artifactFileName)
        {
            // Absolute paths point into the private test directory, so a failing test never writes elsewhere.
            artifactFileName = artifactFileName.Replace("{root}", host.RootDirectory.Replace('\\', '/'));
            using var client = host.CreateAuthenticatedClient();
            var before = SnapshotOutsideArtifacts();
            var payload = Zip.Create(new Dictionary<string, byte[]>
            {
                ["deployment.json"] = Encoding.UTF8.GetBytes(new JObject { ["releaseNotesFileName"] = "releaseNotes.json", ["artifactFileName"] = artifactFileName }.ToString()),
                ["releaseNotes.json"] = Encoding.UTF8.GetBytes(LegacyStore.ReleaseNotesFull),
                [artifactFileName] = Encoding.UTF8.GetBytes("evil"),
            });

            var response = await client.PutAsync("/artifacts/newproduct/windows/x64/1.0",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "newproduct")));
            Assert.Equal(before, SnapshotOutsideArtifacts());
        }

        [Theory]
        [InlineData("../releaseNotes.json")]
        [InlineData("deployment.json")]
        public async Task UploadArtifact_WithUnsafeReleaseNotesFileName_IsRejected(string releaseNotesFileName)
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.Create(new Dictionary<string, byte[]>
            {
                ["deployment.json"] = Encoding.UTF8.GetBytes(new JObject { ["releaseNotesFileName"] = releaseNotesFileName, ["artifactFileName"] = "a.exe" }.ToString()),
                [releaseNotesFileName] = Encoding.UTF8.GetBytes(LegacyStore.ReleaseNotesFull),
                ["a.exe"] = Encoding.UTF8.GetBytes("a"),
            });

            var response = await client.PutAsync("/artifacts/newproduct/windows/x64/1.0",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "newproduct")));
        }

        [Theory]
        [InlineData("/artifacts/new%20product/windows/x64/1.0")]
        [InlineData("/artifacts/.hidden/windows/x64/1.0")]
        [InlineData("/artifacts/newproduct/win%2Fdows/x64/1.0")]
        [InlineData("/artifacts/newproduct/windows/x64/1.0-..%2F..%2Fevil")]
        [InlineData("/artifacts/newproduct/windows/x64/not-a-version")]
        [InlineData("/artifacts/newproduct/windows/x64/1")]
        public async Task UploadArtifact_WithInvalidRouteNames_IsRejected(string url)
        {
            using var client = host.CreateAuthenticatedClient();
            var before = SnapshotOutsideArtifacts();
            var payload = Zip.CreateArtifactPayload("a.exe", Encoding.UTF8.GetBytes("a"), LegacyStore.ReleaseNotesFull);

            var response = await client.PutAsync(url, Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotFound,
                $"Unexpected status {response.StatusCode}");
            Assert.Equal(new[] { "alpha", "pc-s7-link-demo2", "s5-lan-3964R-lan-manager", "teleprof2-os-firmware" },
                Directory.GetDirectories(host.ArtifactDirectory).Select(Path.GetFileName).OrderBy(n => n, StringComparer.Ordinal));
            Assert.Equal(before, SnapshotOutsideArtifacts());
        }

        [Theory]
        [InlineData("../evil", "1.0", "windows/x64")]
        [InlineData("evil/sub", "1.0", "windows/x64")]
        [InlineData(".hidden", "1.0", "windows/x64")]
        [InlineData("foobar", "1.0-../../evil", "windows/x64")]
        [InlineData("foobar", "1.0", "../windows/x64")]
        [InlineData("foobar", "1.0", "windows/..")]
        public async Task UploadPackage_WithUnsafeNames_IsRejected(string identifier, string version, string platform)
        {
            using var client = host.CreateAuthenticatedClient();
            var before = SnapshotOutsideArtifacts();
            var payload = Zip.CreatePackagePayload(identifier.Replace("/", "_"), version, new Dictionary<string, string> { [platform] = "a.exe" });
            payload = ReplaceIdentifier(payload, identifier);

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(before, SnapshotOutsideArtifacts());
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "foobar")));
        }

        [Theory]
        [InlineData("s5-lan-3964R-lan-manager", "Windows", "X86", "1.37")]
        [InlineData("s5-lan-3964R-lan-manager", "windows-xp", "any", "1.8")]
        [InlineData("teleprof2-os-firmware", "teleprof", "2", "8.15.0.0")]
        [InlineData("opc-ua-sdk", "any", "any", "4.0.1-preview.2")]
        [InlineData("Product_Name.v2", "linux", "arm64", "1.10-beta")]
        public async Task UploadArtifact_WithNamesLikeProduction_IsAccepted(string product, string os, string architecture, string version)
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreateArtifactPayload("Setup Tool (x86).exe", Encoding.UTF8.GetBytes("ok"), LegacyStore.ReleaseNotesFull);

            var response = await client.PutAsync($"/artifacts/{product}/{os}/{architecture}/{version}",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var download = await client.GetAsync($"/artifacts/{product}/{os}/{architecture}/{version}");
            Assert.Equal("ok", await download.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UploadPackage_WithAnyPlatform_IsAccepted()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("opc-ua-sdk", "4.0.1", new Dictionary<string, string> { ["any/any"] = "sdk.nupkg" });

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var info = JObject.Parse(await client.GetStringAsync("/artifacts/opc-ua-sdk/info"));
            Assert.Equal("any/any", (string)info["releases"][0]["platforms"][0]);
        }

        [Theory]
        [InlineData("/artifacts/%2E%2E/secret/x/1.0")]
        [InlineData("/artifacts/%2E%2E/%2E%2E/secret/1.0")]
        [InlineData("/artifacts/alpha/%2E%2E/%2E%2E/1.0")]
        [InlineData("/artifacts/%2E%2E/secret/x/latest")]
        public async Task Download_OutsideArtifactDirectory_IsNotFound(string url)
        {
            // A complete "artifact" next to the artifact directory must never be served.
            LegacyStore.AddRelease(host.RootDirectory, "secret", "x", "1.0", "1.0", "secret.txt", LegacyStore.ReleaseNotesFull, pascalCase: false);
            LegacyStore.AddRelease(host.RootDirectory, "", "secret", "x", "1.0", "secret.txt", LegacyStore.ReleaseNotesFull, pascalCase: false);
            using var client = host.CreateClient();

            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("/artifacts/%2E%2E")]
        [InlineData("/artifacts/%2E%2E/backups/x/y")]
        [InlineData("/artifacts/.temp")]
        public async Task Delete_OutsideArtifactDirectory_IsNotFound(string url)
        {
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, ".temp"));
            Directory.CreateDirectory(Path.Combine(host.BackupDirectory, "x", "y"));
            var before = SnapshotOutsideArtifacts();
            using var client = host.CreateAuthenticatedClient();

            var response = await client.DeleteAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.True(Directory.Exists(host.ArtifactDirectory));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, ".temp")));
            Assert.Equal(before, SnapshotOutsideArtifacts());
        }

        [Fact]
        public async Task InternalDirectories_AreNotListedAsProducts()
        {
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, ".temp", Guid.NewGuid().ToString()));
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, ".restore-123"));
            using var client = host.CreateAuthenticatedClient();

            Golden.AssertJson("products", await client.GetStringAsync("/artifacts/"));
            var statistics = JObject.Parse(await client.GetStringAsync("/artifacts/statistics"));
            Assert.Equal(4, (int)statistics["numberOfProducts"]);
            Assert.Equal(10, (int)statistics["numberOfArtifacts"]);
        }

        [Fact]
        public async Task Upload_DoesNotLeaveTemporaryFiles()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe", ["linux/x64"] = "b.tar.gz" });

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, ".temp")));
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "temp")));
        }

        [Fact]
        public async Task BrokenArtifactDirectories_DoNotBreakTheProduct()
        {
            // An incomplete upload (no deployment.json), a directory that is not a version and an artifact
            // directory whose deployment.json points outside of the directory.
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "any", "2.0"));
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "any", "not-a-version"));
            LegacyStore.AddRelease(host.ArtifactDirectory, "alpha", "linux", "arm", "3.0", "a.bin", LegacyStore.ReleaseNotesFull, pascalCase: false);
            File.WriteAllText(Path.Combine(host.ArtifactDirectory, "alpha", "linux", "arm", "3.0", "deployment.json"),
                @"{ ""releaseNotesFileName"": ""../../../../../releaseNotes.json"", ""artifactFileName"": ""a.bin"" }");
            using var client = host.CreateClient();

            var info = await client.GetAsync("/artifacts/alpha/info");
            var latest = await client.GetAsync("/artifacts/alpha/windows/any/latest");

            Assert.Equal(HttpStatusCode.OK, info.StatusCode);
            Golden.AssertJson("info-alpha", await info.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.OK, latest.StatusCode);
        }

        private SortedDictionary<string, byte[]> SnapshotOutsideArtifacts()
        {
            var tree = ReleaseServerTestHost.ReadTree(host.RootDirectory);
            foreach (var key in tree.Keys.Where(key => key.StartsWith("artifacts/", StringComparison.Ordinal)).ToList())
            {
                tree.Remove(key);
            }
            return tree;
        }

        private static byte[] ReplaceIdentifier(byte[] package, string identifier)
        {
            // Rewrite the identifier in package.json (the archive folder name keeps the sanitized identifier).
            var entries = Zip.Read(package);
            var packageJsonKey = entries.Keys.Single(key => key.EndsWith("package.json"));
            var packageJson = JObject.Parse(Encoding.UTF8.GetString(entries[packageJsonKey]));
            packageJson["identifier"] = identifier;
            entries[packageJsonKey] = Encoding.UTF8.GetBytes(packageJson.ToString());
            return Zip.Create(entries.Where(entry => !entry.Key.EndsWith("/")).ToDictionary(entry => entry.Key, entry => entry.Value));
        }
    }
}
