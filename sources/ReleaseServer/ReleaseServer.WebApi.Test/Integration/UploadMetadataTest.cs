//--------------------------------------------------------------------------------------------------
// <copyright file="UploadMetadataTest.cs" company="Traeger Industry Components GmbH">
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

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// ZIP archives created with the macOS Finder or the Windows Explorer contain metadata files. Uploads
    /// ignore them, so they never end up in the artifact directory.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class UploadMetadataTest : IDisposable
    {
        private readonly ReleaseServerTestHost host = new ReleaseServerTestHost();

        public void Dispose()
        {
            host.Dispose();
        }

        private static Dictionary<string, byte[]> OperatingSystemMetadata(string folder)
        {
            var prefix = folder == null ? "" : folder + "/";
            return new Dictionary<string, byte[]>
            {
                [$"__MACOSX/{prefix}._deployment.json"] = LegacyStore.AppleDoubleHeader(),
                [$"__MACOSX/{prefix}._releaseNotes.json"] = LegacyStore.AppleDoubleHeader(),
                [$"{prefix}._setup.exe"] = LegacyStore.AppleDoubleHeader(),
                [$"{prefix}.DS_Store"] = Encoding.UTF8.GetBytes("finder"),
                [$"{prefix}Thumbs.db"] = Encoding.UTF8.GetBytes("thumbs"),
                [$"{prefix}EHTHUMBS.DB"] = Encoding.UTF8.GetBytes("thumbs"),
                [$"{prefix}Desktop.ini"] = Encoding.UTF8.GetBytes("[.ShellClassInfo]"),
                [$"$RECYCLE.BIN/{prefix}deleted.exe"] = Encoding.UTF8.GetBytes("deleted"),
            };
        }

        [Fact]
        public async Task UploadArtifact_FromFinderOrExplorerZip_StoresNoMetadata()
        {
            var entries = new Dictionary<string, byte[]>
            {
                ["deployment.json"] = Encoding.UTF8.GetBytes(LegacyStore.DeploymentCamelCase.Replace("{0}", "setup.exe")),
                ["releaseNotes.json"] = Encoding.UTF8.GetBytes(LegacyStore.ReleaseNotesFull),
                ["setup.exe"] = Encoding.UTF8.GetBytes("setup"),
            };
            foreach (var (name, content) in OperatingSystemMetadata(null))
            {
                entries[name] = content;
            }
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/product/windows/x64/1.0",
                Zip.CreateFormContent("artifact", Zip.Create(entries), "payload.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "product/windows/x64/1.0/deployment.json", "product/windows/x64/1.0/releaseNotes.json", "product/windows/x64/1.0/setup.exe" },
                ReleaseServerTestHost.ReadTree(host.ArtifactDirectory).Keys);
        }

        [Fact]
        public async Task UploadPackage_FromFinderOrExplorerZip_StoresNoMetadata()
        {
            var package = Zip.Read(Zip.CreatePackagePayload("foobar", "1.0", new Dictionary<string, string> { ["windows/x64"] = "setup.exe" }));
            var entries = package.Where(entry => !entry.Key.EndsWith("/")).ToDictionary(entry => entry.Key, entry => entry.Value);
            foreach (var (name, content) in OperatingSystemMetadata("foobar-v1.0"))
            {
                entries[name] = content;
            }
            // A Finder archive also contains the resource fork of package.json, which must not be used
            entries["__MACOSX/foobar-v1.0/._package.json"] = LegacyStore.AppleDoubleHeader();
            entries["__MACOSX/foobar-v1.0/package.json"] = Encoding.UTF8.GetBytes("{ \"not\": \"a package\" }");
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts", Zip.CreateFormContent("package", Zip.Create(entries), "package.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "foobar/windows/x64/1.0/deployment.json", "foobar/windows/x64/1.0/releaseNotes.json", "foobar/windows/x64/1.0/setup.exe" },
                ReleaseServerTestHost.ReadTree(host.ArtifactDirectory).Keys);
        }

        [Theory]
        [InlineData("._setup.exe")]
        [InlineData("Thumbs.db")]
        [InlineData("DESKTOP.INI")]
        public async Task UploadArtifact_ThatIsOnlyMetadata_IsRejected(string artifactFileName)
        {
            using var client = host.CreateAuthenticatedClient();
            var payload = Zip.CreateArtifactPayload(artifactFileName, Encoding.UTF8.GetBytes("metadata"), LegacyStore.ReleaseNotesFull);

            var response = await client.PutAsync("/artifacts/product/windows/x64/1.0",
                Zip.CreateFormContent("artifact", payload, "payload.zip"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Empty(Directory.GetFileSystemEntries(host.ArtifactDirectory));
        }

        [Theory]
        [InlineData("__MACOSX/a/._b.txt", true)]
        [InlineData("folder/__macosx/b.txt", true)]
        [InlineData("._file", true)]
        [InlineData("folder/.DS_Store", true)]
        [InlineData("THUMBS.DB", true)]
        [InlineData("ehthumbs.db", true)]
        [InlineData("sub\\desktop.ini", true)]
        [InlineData("$Recycle.Bin/x.exe", true)]
        [InlineData("setup.exe", false)]
        [InlineData("files/__MACOSX.txt", false)]
        [InlineData("files/thumbs.db.zip", false)]
        [InlineData("files/a._b", false)]
        [InlineData("", false)]
        public void IsOperatingSystemMetadata_RecognizesMetadata(string entryPath, bool expected)
        {
            Assert.Equal(expected, StorageNames.IsOperatingSystemMetadata(entryPath));
        }
    }
}
