//--------------------------------------------------------------------------------------------------
// <copyright file="WriteApiTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Pins the authenticated endpoints: authentication, the files written to disk by uploads, package
    /// uploads and deletions.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class WriteApiTest : IDisposable
    {
        private readonly ReleaseServerTestHost host;

        public WriteApiTest()
        {
            host = new ReleaseServerTestHost();
            LegacyStore.Create(host.ArtifactDirectory);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        public static IEnumerable<object[]> ProtectedRequests()
        {
            yield return new object[] { "PUT", "/artifacts/alpha/windows/x86/1.0" };
            yield return new object[] { "PUT", "/artifacts" };
            yield return new object[] { "DELETE", "/artifacts/alpha/windows/x86/1.0" };
            yield return new object[] { "DELETE", "/artifacts/alpha" };
            yield return new object[] { "GET", "/artifacts/backup" };
            yield return new object[] { "PUT", "/artifacts/restore" };
            yield return new object[] { "GET", "/artifacts/statistics" };
        }

        [Theory]
        [MemberData(nameof(ProtectedRequests))]
        public async Task ProtectedEndpoint_WithoutCredentials_ReturnsUnauthorized(string method, string url)
        {
            using var client = host.CreateClient();
            var before = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);

            var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            ReleaseServerTestHost.AssertSameTree(before, ReleaseServerTestHost.ReadTree(host.ArtifactDirectory));
        }

        [Theory]
        [MemberData(nameof(ProtectedRequests))]
        public async Task ProtectedEndpoint_WithWrongPassword_ReturnsUnauthorized(string method, string url)
        {
            using var client = host.CreateClient();
            var before = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);

            var request = new HttpRequestMessage(new HttpMethod(method), url);
            request.Headers.Authorization = ReleaseServerTestHost.CreateBasicAuthHeader(ReleaseServerTestHost.Username, "wrong");
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            ReleaseServerTestHost.AssertSameTree(before, ReleaseServerTestHost.ReadTree(host.ArtifactDirectory));
        }

        [Theory]
        [InlineData("Basic", "not-base64!")]
        [InlineData("Basic", "dXNlcm5hbWVvbmx5")]
        [InlineData("Bearer", "abc")]
        public async Task ProtectedEndpoint_WithMalformedHeader_ReturnsUnauthorized(string scheme, string parameter)
        {
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Delete, "/artifacts/alpha");
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, parameter);
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha")));
        }

        [Fact]
        public async Task Unauthorized_ResponseBody_MatchesGoldenFile()
        {
            using var client = host.CreateClient();

            var response = await client.DeleteAsync("/artifacts/alpha");

            Golden.AssertText("unauthorized.content-type.txt", response.Content.Headers.ContentType?.ToString() ?? "<none>");
            var body = await response.Content.ReadAsStringAsync();
            Golden.AssertText("unauthorized.body.txt", body.Length == 0 ? "<empty>\n" : Golden.NormalizeJson(body));
        }

        [Fact]
        public async Task GetStatistics_ReturnsCounters()
        {
            using var client = host.CreateAuthenticatedClient();

            var response = await client.GetAsync("/artifacts/statistics");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var statistics = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal(new[] { "disk", "numberOfProducts", "numberOfArtifacts" }, PropertyNames(statistics));
            Assert.Equal(new[] { "totalSize", "usedDiskSpace", "availableFreeSpace" }, PropertyNames((JObject)statistics["disk"]));
            // The empty product directory "pc-s7-link-demo2" has no releases and is no product
            Assert.Equal(3, (int)statistics["numberOfProducts"]);
            Assert.Equal(10, (int)statistics["numberOfArtifacts"]);
        }

        [Fact]
        public async Task UploadArtifact_WritesLegacyCompatibleFiles()
        {
            using var client = host.CreateAuthenticatedClient();
            var artifact = Encoding.UTF8.GetBytes("new artifact");
            var payload = Zip.CreateArtifactPayload("new-setup.exe", artifact, LegacyStore.ReleaseNotesFull);

            var response = await client.PutAsync("/artifacts/newproduct/windows/x64/2.0.1",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var directory = Path.Combine(host.ArtifactDirectory, "newproduct", "windows", "x64", "2.0.1");
            Assert.Equal(new[] { "deployment.json", "new-setup.exe", "releaseNotes.json" }, FileNames(directory));
            Assert.Equal(artifact, File.ReadAllBytes(Path.Combine(directory, "new-setup.exe")));
            Golden.AssertText("stored-artifact.deployment.json.txt", File.ReadAllText(Path.Combine(directory, "deployment.json")));
            Golden.AssertText("stored-artifact.releaseNotes.json.txt", File.ReadAllText(Path.Combine(directory, "releaseNotes.json")));

            // The uploaded artifact is immediately available through the public API.
            var download = await client.GetAsync("/artifacts/newproduct/windows/x64/latest");
            Assert.Equal(artifact, await download.Content.ReadAsByteArrayAsync());
        }

        [Fact]
        public async Task UploadArtifact_ExistingVersion_IsReplaced()
        {
            using var client = host.CreateAuthenticatedClient();
            var artifact = Encoding.UTF8.GetBytes("replacement");
            var payload = Zip.CreateArtifactPayload("replacement.exe", artifact, LegacyStore.ReleaseNotesFull);

            var response = await client.PutAsync("/artifacts/alpha/windows/x86/1.0",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var directory = Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "1.0");
            Assert.Equal(new[] { "deployment.json", "releaseNotes.json", "replacement.exe" }, FileNames(directory));
        }

        [Theory]
        [InlineData("upload-wrong-content-type", "application/octet-stream", true)]
        [InlineData("upload-missing-deployment", "application/zip", false)]
        public async Task UploadArtifact_InvalidPayload_ReturnsBadRequest(string goldenName, string contentType, bool withDeployment)
        {
            using var client = host.CreateAuthenticatedClient();
            var entries = new Dictionary<string, string> { ["releaseNotes.json"] = LegacyStore.ReleaseNotesFull, ["a.exe"] = "a" };
            if (withDeployment)
            {
                entries["deployment.json"] = LegacyStore.DeploymentCamelCase.Replace("{0}", "a.exe");
            }

            var response = await client.PutAsync("/artifacts/newproduct/windows/x64/1.0",
                Zip.CreateFormContent("artifact", Zip.Create(entries), "payload.zip", contentType));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "newproduct")));
        }

        [Fact]
        public async Task UploadArtifact_WithoutFile_ReturnsBadRequest()
        {
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/newproduct/windows/x64/1.0", new MultipartFormDataContent { { new StringContent("x"), "other" } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Golden.AssertJson("upload-without-file", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UploadPackage_StoresAllPlatforms()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("foobar", "2.0.0-preview-2", new Dictionary<string, string>
            {
                ["windows/x64"] = "foobar_win-x64.exe",
                ["linux/arm64"] = "foobar_linux-arm64.tar.gz",
            });

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var windows = Path.Combine(host.ArtifactDirectory, "foobar", "windows", "x64", "2.0.0-preview-2");
            var linux = Path.Combine(host.ArtifactDirectory, "foobar", "linux", "arm64", "2.0.0-preview-2");
            Assert.Equal(new[] { "deployment.json", "foobar_win-x64.exe", "releaseNotes.json" }, FileNames(windows));
            Assert.Equal(new[] { "deployment.json", "foobar_linux-arm64.tar.gz", "releaseNotes.json" }, FileNames(linux));
            Golden.AssertText("stored-package.deployment.json.txt", File.ReadAllText(Path.Combine(windows, "deployment.json")));
            Golden.AssertText("stored-package.releaseNotes.json.txt", File.ReadAllText(Path.Combine(windows, "releaseNotes.json")));

            var info = await client.GetAsync("/artifacts/foobar/info");
            Golden.AssertJson("info-uploaded-package", await info.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UploadPackage_AcceptsWindowsZipContentType()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe" });

            var response = await client.PutAsync("/artifacts",
                Zip.CreateFormContent("package", payload, "package.zip", "application/x-zip-compressed"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task UploadPackage_SameVersionWithoutForce_IsRejected()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe" });
            await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Golden.AssertJson("package-conflict", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task UploadPackage_NewVersionOfExistingProduct_IsAcceptedWithoutForce()
        {
            using var client = host.CreateAuthenticatedClient();
            var first = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe" });
            var second = Zip.CreatePackagePayload("foobar", "1.1", new Dictionary<string, string> { ["windows/x64"] = "b.exe" });
            await client.PutAsync("/artifacts", Zip.CreateFormContent("package", first, "package.zip"));

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", second, "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "foobar", "windows", "x64", "1.0")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "foobar", "windows", "x64", "1.1")));
        }

        [Fact]
        public async Task UploadPackage_ExistingVersionOfLegacyProduct_IsRejectedWithoutForce()
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("alpha", "1.10", new Dictionary<string, string> { ["linux/arm64"] = "a.tar.gz" });

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "linux", "arm64")));
        }

        [Fact]
        public async Task UploadPackage_SameVersionWithForce_ReplacesFiles()
        {
            using var client = host.CreateAuthenticatedClient();
            var first = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe" });
            var second = Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "b.exe" });
            await client.PutAsync("/artifacts", Zip.CreateFormContent("package", first, "package.zip"));

            var response = await client.PutAsync("/artifacts?force=true", Zip.CreateFormContent("package", second, "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "b.exe", "deployment.json", "releaseNotes.json" },
                FileNames(Path.Combine(host.ArtifactDirectory, "foobar", "windows", "x64", "1.0")));
        }

        [Theory]
        [InlineData("package-missing-package-json", false, "windows/x64")]
        [InlineData("package-invalid-platform", true, "windows")]
        public async Task UploadPackage_InvalidPackage_ReturnsBadRequest(string goldenName, bool withPackageJson, string platform)
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = withPackageJson
                ? Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { [platform] = "a.exe" })
                : Zip.Create(new Dictionary<string, string> { ["foobar/a.exe"] = "a" });

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "foobar")));
        }

        [Fact]
        public async Task DeleteSpecificArtifact_RemovesOnlyThisArtifact()
        {
            using var client = host.CreateAuthenticatedClient();

            var response = await client.DeleteAsync("/artifacts/alpha/windows/x86/1.0");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "1.0")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x64", "1.0")));

            var again = await client.DeleteAsync("/artifacts/alpha/windows/x86/1.0");
            Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
            Golden.AssertJson("delete-artifact-not-found", await again.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task DeleteSpecificArtifact_LastRelease_RemovesEmptyParentDirectories()
        {
            using var client = host.CreateAuthenticatedClient();

            Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync("/artifacts/s5-lan-3964R-lan-manager/Windows/X86/1.36")).StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "s5-lan-3964R-lan-manager", "Windows")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "s5-lan-3964R-lan-manager", "windows-xp", "any", "1.7")));

            Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync("/artifacts/s5-lan-3964R-lan-manager/windows-xp/any/1.7")).StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "s5-lan-3964R-lan-manager")));
            Assert.DoesNotContain("s5-lan-3964R-lan-manager", await client.GetStringAsync("/artifacts/"));
            Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync("/artifacts/s5-lan-3964R-lan-manager/info")).StatusCode);
        }

        [Fact]
        public async Task DeleteSpecificArtifact_KeepsDirectoriesWithOtherContent()
        {
            File.WriteAllText(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "notes.txt"), "kept");
            using var client = host.CreateAuthenticatedClient();

            await client.DeleteAsync("/artifacts/alpha/windows/x86/1.0");

            Assert.True(File.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "notes.txt")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x64", "1.0")));
        }

        [Fact]
        public async Task UploadPackage_ThenDeleteAllPlatforms_LeavesNoPhantomProduct()
        {
            // The flow of the client: upload a package, delete the release on every platform
            using var client = host.CreateAuthenticatedClient();
            var platforms = new Dictionary<string, string> { ["windows/x64"] = "a.exe", ["windows/arm64"] = "b.exe", ["linux/arm32"] = "c.tar.gz" };
            await client.PutAsync("/artifacts", Zip.CreateFormContent("package", Zip.CreatePackagePayload("foobar", "2.0.0-preview-2", platforms), "package.zip"));

            foreach (var platform in platforms.Keys)
            {
                Assert.Equal(HttpStatusCode.OK, (await client.DeleteAsync($"/artifacts/foobar/{platform}/2.0.0-preview-2")).StatusCode);
            }

            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "foobar")));
            Assert.DoesNotContain("foobar", await client.GetStringAsync("/artifacts/"));
        }

        [Fact]
        public async Task DeleteProduct_RemovesAllArtifactsOfProduct()
        {
            using var client = host.CreateAuthenticatedClient();

            var response = await client.DeleteAsync("/artifacts/alpha");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "alpha")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "teleprof2-os-firmware")));

            var again = await client.DeleteAsync("/artifacts/alpha");
            Assert.Equal(HttpStatusCode.NotFound, again.StatusCode);
            Golden.AssertJson("delete-product-not-found", await again.Content.ReadAsStringAsync());
        }

        private static string[] FileNames(string directory)
        {
            var names = Array.ConvertAll(Directory.GetFiles(directory), Path.GetFileName);
            Array.Sort(names, StringComparer.Ordinal);
            return names;
        }

        private static string[] PropertyNames(JObject obj)
        {
            var names = new List<string>();
            foreach (var property in obj.Properties())
            {
                names.Add(property.Name);
            }
            return names.ToArray();
        }
    }
}
