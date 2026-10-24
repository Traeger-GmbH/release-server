//--------------------------------------------------------------------------------------------------
// <copyright file="RestoreValidationTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Tests the transactional restore: invalid backups are rejected before the existing data is touched,
    /// and the backup directory keeps a limited number of backups.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class RestoreValidationTest : IDisposable
    {
        private readonly ReleaseServerTestHost host;

        public RestoreValidationTest()
        {
            host = new ReleaseServerTestHost();
            LegacyStore.Create(host.ArtifactDirectory);
        }

        public void Dispose()
        {
            host.Dispose();
        }

        public static IEnumerable<object[]> InvalidBackups()
        {
            var valid = ValidBackupEntries();

            yield return Case("not a ZIP archive", null, Encoding.UTF8.GetBytes("this is not a zip file"));
            yield return Case("empty archive", new Dictionary<string, byte[]>());
            yield return Case("only empty product directories", new Dictionary<string, byte[]> { ["product/"] = new byte[0] });
            yield return Case("missing deployment.json", Without(valid, "restored/windows/x64/1.0/deployment.json"));
            yield return Case("invalid deployment.json", With(valid, "restored/windows/x64/1.0/deployment.json", "{ invalid"));
            yield return Case("deployment.json pointing outside", With(valid, "restored/windows/x64/1.0/deployment.json",
                @"{ ""releaseNotesFileName"": ""../../releaseNotes.json"", ""artifactFileName"": ""a.exe"" }"));
            yield return Case("missing release notes", Without(valid, "restored/windows/x64/1.0/releaseNotes.json"));
            yield return Case("invalid release notes", With(valid, "restored/windows/x64/1.0/releaseNotes.json", @"{ ""changes"": {} }"));
            yield return Case("missing artifact file", Without(valid, "restored/windows/x64/1.0/a.exe"));
            yield return Case("entry outside of the directory", With(valid, "../evil.txt", "evil"));
            yield return Case("one broken artifact among valid ones",
                With(With(valid, "restored/linux/x64/3.0/deployment.json", LegacyStore.DeploymentCamelCase.Replace("{0}", "c.tar.gz")),
                    "restored/linux/x64/3.0/releaseNotes.json", LegacyStore.ReleaseNotesFull));
        }

        [Theory]
        [MemberData(nameof(InvalidBackups))]
        public async Task Restore_InvalidBackup_IsRejectedAndKeepsData(string description, byte[] backup)
        {
            var before = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);
            var outsideBefore = Directory.GetFileSystemEntries(host.RootDirectory).OrderBy(e => e, StringComparer.Ordinal).ToList();
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.True(HttpStatusCode.BadRequest == response.StatusCode, $"{description}: unexpected status {response.StatusCode}");
            Assert.Contains("application/problem+json", response.Content.Headers.ContentType.ToString());
            ReleaseServerTestHost.AssertSameTree(before, ReleaseServerTestHost.ReadTree(host.ArtifactDirectory));
            Assert.Equal(outsideBefore, Directory.GetFileSystemEntries(host.RootDirectory).OrderBy(e => e, StringComparer.Ordinal).ToList());
        }

        public static IEnumerable<object[]> HarmlessBackups()
        {
            var valid = ValidBackupEntries();

            // Previous versions extracted the complete upload archive into the artifact directory, e.g. with
            // macOS metadata (exactly like testproduct/windows/arm64/1.0.0 in production).
            yield return Case("sub directory with macOS metadata in an artifact directory",
                With(valid, "restored/windows/x64/1.0/__MACOSX/._releaseNotes.json", "AppleDouble"));
            yield return Case("additional files in an artifact directory",
                With(With(valid, "restored/windows/x64/1.0/readme.txt", "x"), "restored/windows/x64/1.0/docs/manual.pdf", "pdf"));
            yield return Case("artifact file in a sub directory",
                With(Without(With(valid, "restored/windows/x64/1.0/bin/a.exe", "a"), "restored/windows/x64/1.0/a.exe"),
                    "restored/windows/x64/1.0/deployment.json", LegacyStore.DeploymentCamelCase.Replace("{0}", "bin/a.exe")));
            yield return Case("file in the root directory", With(valid, "readme.txt", "x"));
            yield return Case("file in a product directory", With(valid, "restored/readme.txt", "x"));
            yield return Case("version directory is not a version", Rename(valid, "restored/windows/x64/1.0/", "restored/windows/x64/latest/"));
            yield return Case("files in an architecture directory", Rename(valid, "restored/windows/x64/1.0/", "restored/windows/1.0/"));
        }

        [Theory]
        [MemberData(nameof(HarmlessBackups))]
        public async Task Restore_UnknownButHarmlessStructure_IsAcceptedUnchanged(string description, byte[] backup)
        {
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.True(HttpStatusCode.OK == response.StatusCode, $"{description}: {response.StatusCode} {await response.Content.ReadAsStringAsync()}");
            ReleaseServerTestHost.AssertSameTree(Zip.Read(backup), ReleaseServerTestHost.ReadTree(host.ArtifactDirectory));
        }

        [Fact]
        public async Task BackupOfKnownSpecialCases_IsRestorableByteIdentical()
        {
            // All special cases known from production data, including the extracted macOS metadata.
            LegacyStore.CreateKnownSpecialCases(host.ArtifactDirectory);
            var expected = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);
            using var client = host.CreateAuthenticatedClient();
            var backup = await client.GetByteArrayAsync("/artifacts/backup");

            // Restore into the same server (the data is replaced by itself) and into a new server
            var sameServer = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));
            using var otherHost = new ReleaseServerTestHost();
            using var otherClient = otherHost.CreateAuthenticatedClient();
            var otherServer = await otherClient.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, sameServer.StatusCode);
            Assert.Equal(HttpStatusCode.OK, otherServer.StatusCode);
            ReleaseServerTestHost.AssertSameTree(expected, ReleaseServerTestHost.ReadTree(host.ArtifactDirectory));
            ReleaseServerTestHost.AssertSameTree(expected, ReleaseServerTestHost.ReadTree(otherHost.ArtifactDirectory));
            Assert.Equal(LegacyStore.ArtifactContent("testproduct/windows/arm64/1.0.0"),
                await otherClient.GetByteArrayAsync("/artifacts/testproduct/windows/arm64/1.0.0"));
            Assert.Equal(LegacyStore.ArtifactContent("legacy-subfolder/windows/x64/0.9"),
                await otherClient.GetByteArrayAsync("/artifacts/legacy-subfolder/windows/x64/latest"));
        }

        [Fact]
        public async Task Restore_InvalidBackup_ExplainsTheProblem()
        {
            using var client = host.CreateAuthenticatedClient();
            var backup = Zip.Create(Without(ValidBackupEntries(), "restored/windows/x64/1.0/a.exe"));

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Golden.AssertJson("restore-invalid-backup", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task Restore_ValidBackup_ReplacesDataAndLeavesNoInternalDirectories()
        {
            using var client = host.CreateAuthenticatedClient();
            var backup = Zip.Create(ValidBackupEntries());

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", backup, "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(new[] { "empty-product", "restored" },
                Directory.GetFileSystemEntries(host.ArtifactDirectory).Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal("a", await client.GetStringAsync("/artifacts/restored/windows/x64/1.0"));
        }

        [Fact]
        public async Task Restore_LegacyBackupWithTemporaryDirectory_SkipsTemporaryData()
        {
            // Old servers left "temp/<guid>" behind after failed uploads; such backups must stay restorable.
            var entries = With(ValidBackupEntries(), $"temp/{Guid.NewGuid()}/half-written.exe", "partial");
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", Zip.Create(entries), "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, "temp")));
            Assert.True(Directory.Exists(Path.Combine(host.ArtifactDirectory, "restored")));
        }

        [Fact]
        public async Task Restore_ProductNamedTemp_IsRestored()
        {
            var entries = Rename(ValidBackupEntries(), "restored/", "temp/");
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", Zip.Create(entries), "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("a", await client.GetStringAsync("/artifacts/temp/windows/x64/1.0"));
        }

        [Fact]
        public async Task Restore_KeepsInternalDirectoriesOutOfTheData()
        {
            var entries = With(ValidBackupEntries(), ".temp/x/y.txt", "x");
            using var client = host.CreateAuthenticatedClient();

            var response = await client.PutAsync("/artifacts/restore", Zip.CreateFormContent("backupFile", Zip.Create(entries), "backup.zip"));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(Directory.Exists(Path.Combine(host.ArtifactDirectory, ".temp")));
        }

        [Fact]
        public async Task Backup_ExcludesInternalDirectories()
        {
            Directory.CreateDirectory(Path.Combine(host.ArtifactDirectory, ".temp", "upload"));
            File.WriteAllText(Path.Combine(host.ArtifactDirectory, ".temp", "upload", "partial.exe"), "partial");
            var expected = ReleaseServerTestHost.ReadTree(host.ArtifactDirectory);
            foreach (var key in expected.Keys.Where(key => key.StartsWith(".temp/", StringComparison.Ordinal)).ToList())
            {
                expected.Remove(key);
            }
            using var client = host.CreateAuthenticatedClient();

            var backup = await client.GetByteArrayAsync("/artifacts/backup");

            ReleaseServerTestHost.AssertSameTree(expected, Zip.Read(backup));
        }

        [Fact]
        public async Task Backup_HasSameLayoutAsLegacyBackup()
        {
            // The legacy server created backups with ZipFile.CreateFromDirectory.
            var legacyBackup = Path.Combine(host.RootDirectory, "legacy.zip");
            ZipFile.CreateFromDirectory(host.ArtifactDirectory, legacyBackup);
            using var client = host.CreateAuthenticatedClient();

            var backup = await client.GetByteArrayAsync("/artifacts/backup");

            using var actual = new ZipArchive(new MemoryStream(backup));
            using var expected = ZipFile.OpenRead(legacyBackup);
            Assert.Equal(
                expected.Entries.Select(entry => entry.FullName).OrderBy(name => name, StringComparer.Ordinal),
                actual.Entries.Select(entry => entry.FullName).OrderBy(name => name, StringComparer.Ordinal));
        }

        [Fact]
        public async Task Backup_KeepsConfiguredNumberOfBackups()
        {
            using var limitedHost = new ReleaseServerTestHost(new Dictionary<string, string> { ["BackupRetentionCount"] = "2" });
            LegacyStore.Create(limitedHost.ArtifactDirectory);
            using var client = limitedHost.CreateAuthenticatedClient();
            CreateOldBackupFiles(limitedHost.BackupDirectory, 3);

            await client.GetByteArrayAsync("/artifacts/backup");

            var backups = BackupFiles(limitedHost.BackupDirectory);
            Assert.Equal(2, backups.Count);
            Assert.Equal("backup_2020-01-03_00-00-00.zip", backups[0]);
            Assert.StartsWith("backup_20", backups[1]);
            Assert.NotEqual("backup_2020-01-02_00-00-00.zip", backups[1]);
        }

        [Fact]
        public async Task Backup_KeepsFiveBackupsByDefault()
        {
            using var client = host.CreateAuthenticatedClient();
            CreateOldBackupFiles(host.BackupDirectory, 6);
            File.WriteAllText(Path.Combine(host.BackupDirectory, "notes.txt"), "not a backup");

            var first = await client.GetAsync("/artifacts/backup");
            var fileName = first.Content.Headers.ContentDisposition.FileName.Trim('"');

            var backups = BackupFiles(host.BackupDirectory);
            Assert.Equal(5, backups.Count);
            Assert.Equal(fileName, backups.Last());
            Assert.Equal(new[] { "backup_2020-01-03_00-00-00.zip", "backup_2020-01-04_00-00-00.zip", "backup_2020-01-05_00-00-00.zip", "backup_2020-01-06_00-00-00.zip" },
                backups.Take(4));
            Assert.True(File.Exists(Path.Combine(host.BackupDirectory, "notes.txt")));
        }

        [Fact]
        public async Task Backup_TwoBackupsInSameSecond_DoNotOverwriteEachOther()
        {
            using var client = host.CreateAuthenticatedClient();

            var first = await client.GetAsync("/artifacts/backup");
            var second = await client.GetAsync("/artifacts/backup");

            Assert.NotEqual(first.Content.Headers.ContentDisposition.FileName, second.Content.Headers.ContentDisposition.FileName);
            Assert.Equal(2, BackupFiles(host.BackupDirectory).Count);
        }

        private static void CreateOldBackupFiles(string backupDirectory, int count)
        {
            for (var day = 1; day <= count; day++)
            {
                File.WriteAllText(Path.Combine(backupDirectory, $"backup_2020-01-{day:00}_00-00-00.zip"), "old backup");
            }
        }

        private static List<string> BackupFiles(string backupDirectory)
        {
            return Directory.GetFiles(backupDirectory, "backup_*.zip").Select(Path.GetFileName).OrderBy(name => name, StringComparer.Ordinal).ToList();
        }

        private static Dictionary<string, byte[]> ValidBackupEntries()
        {
            return new Dictionary<string, byte[]>
            {
                ["restored/windows/x64/1.0/deployment.json"] = Encoding.UTF8.GetBytes(LegacyStore.DeploymentPascalCase.Replace("{0}", "a.exe")),
                ["restored/windows/x64/1.0/releaseNotes.json"] = Encoding.UTF8.GetBytes(LegacyStore.ReleaseNotesFull),
                ["restored/windows/x64/1.0/a.exe"] = Encoding.UTF8.GetBytes("a"),
                ["restored/linux/x64/2.0/deployment.json"] = Encoding.UTF8.GetBytes(LegacyStore.DeploymentCamelCase.Replace("{0}", "b.tar.gz")),
                ["restored/linux/x64/2.0/releaseNotes.json"] = Encoding.UTF8.GetBytes(LegacyStore.ReleaseNotesGermanOnly),
                ["restored/linux/x64/2.0/b.tar.gz"] = Encoding.UTF8.GetBytes("b"),
                ["empty-product/"] = new byte[0],
            };
        }

        private static object[] Case(string description, Dictionary<string, byte[]> entries, byte[] raw = null)
        {
            return new object[] { description, raw ?? Zip.Create(entries) };
        }

        private static Dictionary<string, byte[]> With(Dictionary<string, byte[]> entries, string name, string content)
        {
            return new Dictionary<string, byte[]>(entries) { [name] = Encoding.UTF8.GetBytes(content) };
        }

        private static Dictionary<string, byte[]> Without(Dictionary<string, byte[]> entries, string name)
        {
            var result = new Dictionary<string, byte[]>(entries);
            result.Remove(name);
            return result;
        }

        private static Dictionary<string, byte[]> Rename(Dictionary<string, byte[]> entries, string oldPrefix, string newPrefix)
        {
            return entries.ToDictionary(
                entry => entry.Key.StartsWith(oldPrefix, StringComparison.Ordinal) ? newPrefix + entry.Key.Substring(oldPrefix.Length) : entry.Key,
                entry => entry.Value);
        }
    }
}
