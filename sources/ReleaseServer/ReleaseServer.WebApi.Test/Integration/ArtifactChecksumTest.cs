//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactChecksumTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Tests the field "artifacts" of /info (file name, size, SHA-256, download URL), the checksum stored in
    /// deployment.json and the background calculation of checksums for existing data.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class ArtifactChecksumTest : IDisposable
    {
        private readonly List<ReleaseServerTestHost> hosts = new List<ReleaseServerTestHost>();

        public void Dispose()
        {
            hosts.ForEach(host => host.Dispose());
        }

        private ReleaseServerTestHost CreateHost(bool addMissingHashes)
        {
            var host = new ReleaseServerTestHost(new Dictionary<string, string> { ["AddMissingArtifactHashes"] = addMissingHashes ? "true" : "false" });
            hosts.Add(host);
            return host;
        }

        private static string Sha256(byte[] content)
        {
            return Convert.ToHexString(SHA256.HashData(content)).ToLowerInvariant();
        }

        [Fact]
        public async Task UploadArtifact_StoresChecksumAndListsArtifact()
        {
            var host = CreateHost(addMissingHashes: false);
            using var client = host.CreateAuthenticatedClient();
            var content = Encoding.UTF8.GetBytes("setup content");
            var payload = Zip.CreateArtifactPayload("Setup Tool.exe", content, LegacyStore.ReleaseNotesFull);

            await client.PutAsync("/artifacts/product/windows/x64/1.2.0", Zip.CreateFormContent("artifact", payload, "payload.zip"));

            var deployment = JObject.Parse(File.ReadAllText(Path.Combine(host.ArtifactDirectory, "product", "windows", "x64", "1.2.0", "deployment.json")));
            Assert.Equal(Sha256(content), (string)deployment["Sha256"]);

            var info = JObject.Parse(await client.GetStringAsync("/artifacts/product/info"));
            var artifact = (JObject)info["releases"][0]["artifacts"].Single();
            Assert.Equal(new[] { "platform", "fileName", "size", "sha256", "downloadUrl" }, artifact.Properties().Select(p => p.Name));
            Assert.Equal("windows/x64", (string)artifact["platform"]);
            Assert.Equal("Setup Tool.exe", (string)artifact["fileName"]);
            Assert.Equal(content.Length, (long)artifact["size"]);
            Assert.Equal(Sha256(content), (string)artifact["sha256"]);
            Assert.Equal("/artifacts/product/windows/x64/1.2.0", (string)artifact["downloadUrl"]);
            Assert.Equal(content, await client.GetByteArrayAsync((string)artifact["downloadUrl"]));
        }

        [Fact]
        public async Task UploadPackage_StoresChecksumPerPlatform()
        {
            var host = CreateHost(addMissingHashes: false);
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreatePackagePayload("foobar", "2.0.0", new Dictionary<string, string> { ["windows/x64"] = "a.exe", ["any/any"] = "docs.zip" });

            await client.PutAsync("/artifacts", Zip.CreateFormContent("package", payload, "package.zip"));

            var info = JObject.Parse(await client.GetStringAsync("/artifacts/foobar/info"));
            foreach (var artifact in info["releases"][0]["artifacts"])
            {
                var downloaded = await client.GetByteArrayAsync((string)artifact["downloadUrl"]);
                Assert.Equal(Sha256(downloaded), (string)artifact["sha256"]);
                Assert.Equal(downloaded.Length, (long)artifact["size"]);
            }
        }

        [Fact]
        public async Task ExistingData_WithoutBackgroundCalculation_HasSizeButNoChecksum()
        {
            var host = CreateHost(addMissingHashes: false);
            LegacyStore.Create(host.ArtifactDirectory);
            using var client = host.CreateClient();

            var info = JObject.Parse(await client.GetStringAsync("/artifacts/teleprof2-os-firmware/info"));
            var artifact = info["releases"][0]["artifacts"][0];

            Assert.Equal(JTokenType.Null, artifact["sha256"].Type);
            Assert.Equal(LegacyStore.ArtifactContent("teleprof2-os-firmware/teleprof/2/8.14.0.1").Length, (long)artifact["size"]);
        }

        [Fact]
        public async Task ExistingData_ChecksumsAreAddedWithoutChangingAnythingElse()
        {
            var host = CreateHost(addMissingHashes: true);
            LegacyStore.CreateKnownSpecialCases(host.ArtifactDirectory);
            var before = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);
            using var client = host.CreateClient();

            await WaitForChecksums(client, "alpha", "testproduct", "legacy-subfolder", "s5-lan-3964R-lan-manager", "teleprof2-os-firmware");

            var after = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);
            Assert.Equal(before.Keys, after.Keys);
            foreach (var (path, content) in before)
            {
                if (!path.EndsWith("/deployment.json"))
                {
                    Assert.True(content.SequenceEqual(after[path]), $"{path} was changed");
                    continue;
                }

                // deployment.json: all existing properties and values are kept, only the checksum is added
                var original = JObject.Parse(Encoding.UTF8.GetString(content));
                var updated = JObject.Parse(Encoding.UTF8.GetString(after[path]));
                var checksumProperty = original.Properties().Any(p => char.IsUpper(p.Name[0])) ? "Sha256" : "sha256";
                Assert.Equal(original.Properties().Select(p => p.Name).Append(checksumProperty), updated.Properties().Select(p => p.Name));
                foreach (var property in original.Properties())
                {
                    Assert.Equal(property.Value, updated[property.Name]);
                }

                var directory = Path.GetDirectoryName(Path.Combine(host.ArtifactDirectory, path));
                var artifactFile = Path.Combine(directory, (string)(original["artifactFileName"] ?? original["ArtifactFileName"]));
                Assert.Equal(Sha256(File.ReadAllBytes(artifactFile)), (string)updated[checksumProperty]);
            }

            // Idempotent: a second run adds nothing
            var service = host.Services.GetRequiredService<IReleaseArtifactService>();
            Assert.Equal(0, await service.AddMissingArtifactHashes(CancellationToken.None));
        }

        [Fact]
        public async Task ExistingData_BrokenReleaseDirectories_AreSkipped()
        {
            var host = CreateHost(addMissingHashes: false);
            LegacyStore.Create(host.ArtifactDirectory);
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "any", "3.0"));
            File.Delete(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "1.0", "alpha-setup.exe"));
            var service = host.Services.GetRequiredService<IReleaseArtifactService>();

            var added = await service.AddMissingArtifactHashes(CancellationToken.None);

            // 10 artifacts in the legacy data set, one of them without its file
            Assert.Equal(9, added);
            Assert.False(File.Exists(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "any", "3.0", "deployment.json")));
            Assert.DoesNotContain("ha256", File.ReadAllText(Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "1.0", "deployment.json")));
        }

        [Fact]
        public void ChecksumOfChangedArtifact_IsNotStored()
        {
            var host = CreateHost(addMissingHashes: false);
            LegacyStore.Create(host.ArtifactDirectory);
            var repository = new FsReleaseArtifactRepository(
                NSubstitute.Substitute.For<Microsoft.Extensions.Logging.ILogger<FsReleaseArtifactRepository>>(),
                new DirectoryInfo(host.ArtifactDirectory), new DirectoryInfo(host.BackupDirectory));
            var directory = Path.Combine(host.ArtifactDirectory, "teleprof2-os-firmware", "teleprof", "2", "8.14.0.1");

            var hash = repository.CalculateArtifactHash(directory);
            File.WriteAllText(Path.Combine(directory, "firmware.bin"), "replaced while the checksum was calculated");

            Assert.False(repository.StoreArtifactHash(hash));
            Assert.DoesNotContain("ha256", File.ReadAllText(Path.Combine(directory, "deployment.json")));
        }

        [Fact]
        public async Task Download_WithChecksum_SendsETagAndDigest()
        {
            var host = CreateHost(addMissingHashes: false);
            using var client = host.CreateAuthenticatedClient();
            var content = Encoding.UTF8.GetBytes("download content");
            await client.PutAsync("/artifacts/product/windows/x64/1.0", Zip.CreateFormContent("artifact",
                Zip.CreateArtifactPayload("a.exe", content, LegacyStore.ReleaseNotesFull), "payload.zip"));

            var response = await client.GetAsync("/artifacts/product/windows/x64/latest");

            Assert.Equal($"\"{Sha256(content)}\"", response.Headers.ETag.Tag);
            Assert.Equal($"sha-256=:{Convert.ToBase64String(SHA256.HashData(content))}:", response.Headers.GetValues("Repr-Digest").Single());

            using var conditional = new HttpRequestMessage(HttpMethod.Get, "/artifacts/product/windows/x64/1.0");
            conditional.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{Sha256(content)}\""));
            Assert.Equal(HttpStatusCode.NotModified, (await client.SendAsync(conditional)).StatusCode);
        }

        [Fact]
        public async Task Download_WithoutChecksum_SendsNoETag()
        {
            var host = CreateHost(addMissingHashes: false);
            LegacyStore.Create(host.ArtifactDirectory);
            using var client = host.CreateClient();

            var response = await client.GetAsync("/artifacts/alpha/windows/x86/1.0");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Null(response.Headers.ETag);
            Assert.False(response.Headers.Contains("Repr-Digest"));
        }

        [Fact]
        public async Task Restore_AddsChecksumsOfRestoredData()
        {
            var source = CreateHost(addMissingHashes: false);
            LegacyStore.Create(source.ArtifactDirectory);
            using var sourceClient = source.CreateAuthenticatedClient();
            var backup = await sourceClient.GetByteArrayAsync("/artifacts/backup");
            var target = CreateHost(addMissingHashes: true);
            using var targetClient = target.CreateAuthenticatedClient();

            var response = await targetClient.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            await WaitForChecksums(targetClient, "alpha", "teleprof2-os-firmware");
        }

        [Fact]
        public async Task DownloadUrl_UsesTheVersionDirectoryName()
        {
            // A directory name that is not the normalized version ("01.2" is version "1.2")
            var host = CreateHost(addMissingHashes: false);
            LegacyStore.AddRelease(host.ArtifactDirectory, "old-product", "windows", "any", "01.2", "a.zip", LegacyStore.ReleaseNotesFull, pascalCase: false);
            using var client = host.CreateClient();

            var info = JObject.Parse(await client.GetStringAsync("/artifacts/old-product/info"));
            var release = info["releases"][0];

            Assert.Equal("1.2", (string)release["version"]);
            Assert.Equal("/artifacts/old-product/windows/any/01.2", (string)release["artifacts"][0]["downloadUrl"]);
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync((string)release["artifacts"][0]["downloadUrl"])).StatusCode);
        }

        private static async Task WaitForChecksums(HttpClient client, params string[] products)
        {
            for (var attempt = 0; attempt < 100; attempt++)
            {
                var complete = true;
                foreach (var product in products)
                {
                    var info = JObject.Parse(await client.GetStringAsync($"/artifacts/{product}/info?limit=1000"));
                    complete &= info["releases"].SelectMany(release => release["artifacts"]).All(artifact => artifact["sha256"].Type == JTokenType.String);
                }

                if (complete)
                    return;

                await Task.Delay(100);
            }

            Assert.Fail("The checksums were not added in time.");
        }
    }
}
