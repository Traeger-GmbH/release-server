//--------------------------------------------------------------------------------------------------
// <copyright file="BackupRestoreTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Pins backup and restore: a backup contains the complete artifact directory and restoring it
    /// (also into another server instance) reproduces the data byte by byte.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class BackupRestoreTest : IDisposable
    {
        private readonly ReleaseServerTestHost source;
        private readonly ReleaseServerTestHost target;

        public BackupRestoreTest()
        {
            source = new ReleaseServerTestHost();
            LegacyStore.Create(source.ArtifactDirectory);
            target = new ReleaseServerTestHost();
        }

        public void Dispose()
        {
            source.Dispose();
            target.Dispose();
        }

        [Fact]
        public async Task Backup_ContainsCompleteArtifactDirectory()
        {
            using var client = source.CreateAuthenticatedClient();

            var response = await client.GetAsync("/artifacts/backup");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/x-zip-compressed", response.Content.Headers.ContentType.ToString());
            var contentDisposition = response.Content.Headers.ContentDisposition.ToString();
            Assert.Matches(new Regex(@"^attachment; filename=backup_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\.zip; filename\*=UTF-8''backup_\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2}\.zip$"), contentDisposition);

            var backup = Zip.Read(await response.Content.ReadAsByteArrayAsync());
            ReleaseServerTestHost.AssertSameTree(ReleaseServerTestHost.ReadTree(source.ArtifactDirectory), backup);
        }

        [Fact]
        public async Task Backup_IsStoredInBackupDirectory()
        {
            using var client = source.CreateAuthenticatedClient();

            var response = await client.GetAsync("/artifacts/backup");
            var fileName = response.Content.Headers.ContentDisposition.FileName.Trim('"');

            Assert.True(File.Exists(Path.Combine(source.BackupDirectory, fileName)));
        }

        [Fact]
        public async Task Restore_BackupOfOtherInstance_ReproducesDataExactly()
        {
            using var sourceClient = source.CreateAuthenticatedClient();
            using var targetClient = target.CreateAuthenticatedClient();
            var backup = await (await sourceClient.GetAsync("/artifacts/backup")).Content.ReadAsByteArrayAsync();

            var response = await targetClient.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ReleaseServerTestHost.AssertSameTree(
                ReleaseServerTestHost.ReadTree(source.ArtifactDirectory),
                ReleaseServerTestHost.ReadTree(target.ArtifactDirectory));

            var info = await targetClient.GetAsync("/artifacts/alpha/info");
            Golden.AssertJson("info-alpha", await info.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Restore_LegacyBackupArchive_ReproducesDataExactly()
        {
            // A backup as created by the existing server: ZipFile.CreateFromDirectory of the artifact root.
            var legacyBackup = Path.Combine(source.RootDirectory, "legacy-backup.zip");
            ZipFile.CreateFromDirectory(source.ArtifactDirectory, legacyBackup);
            using var client = target.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore",
                Zip.CreateFormContent("backupFile", File.ReadAllBytes(legacyBackup), "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            ReleaseServerTestHost.AssertSameTree(
                ReleaseServerTestHost.ReadTree(source.ArtifactDirectory),
                ReleaseServerTestHost.ReadTree(target.ArtifactDirectory));
        }

        [Fact]
        public async Task Restore_ReplacesExistingData()
        {
            LegacyStore.AddRelease(target.ArtifactDirectory, "obsolete", "windows", "any", "1.0", "o.exe", LegacyStore.ReleaseNotesFull, pascalCase: false);
            using var sourceClient = source.CreateAuthenticatedClient();
            using var targetClient = target.CreateAuthenticatedClient();
            var backup = await (await sourceClient.GetAsync("/artifacts/backup")).Content.ReadAsByteArrayAsync();

            var response = await targetClient.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(target.ArtifactDirectory, "obsolete")));
        }

        [Fact]
        public async Task Restore_WithoutFile_ReturnsBadRequest()
        {
            using var client = target.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", new System.Net.Http.MultipartFormDataContent { { new System.Net.Http.StringContent("x"), "other" } });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Golden.AssertJson("restore-without-file", await response.Content.ReadAsStringAsync());
        }
    }
}
