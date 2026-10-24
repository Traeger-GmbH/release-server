//--------------------------------------------------------------------------------------------------
// <copyright file="StoreArtifactFailureTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

using NSubstitute;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test
{
    /// <summary>
    /// A failing upload (e.g. a broken connection while reading the payload) must not leave temporary
    /// files or a partial artifact behind.
    /// </summary>
    public sealed class StoreArtifactFailureTest : IDisposable
    {
        private readonly DirectoryInfo root = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "release-server-tests", Guid.NewGuid().ToString("N")));

        public void Dispose()
        {
            if (root.Exists)
                root.Delete(true);
        }

        [Fact]
        public void StoreArtifact_ReadError_LeavesNoTemporaryFiles()
        {
            var artifacts = root.CreateSubdirectory("artifacts");
            var repository = new FsReleaseArtifactRepository(
                Substitute.For<ILogger<FsReleaseArtifactRepository>>(), artifacts, root.CreateSubdirectory("backups"));

            using var artifact = new ReleaseArtifact
            {
                DeploymentInformation = new DeploymentInformation
                {
                    Identifier = "product",
                    Os = "windows",
                    Architecture = "x64",
                    Version = new ProductVersion("1.0"),
                    ReleaseNotes = new ReleaseNotes()
                },
                DeploymentMetaInformation = new DeploymentMetaInformation { ArtifactFileName = "a.exe", ReleaseNotesFileName = "releaseNotes.json" },
                Content = new FailingStream()
            };

            Assert.Throws<IOException>(() => repository.StoreArtifact(artifact));

            Assert.Empty(artifacts.EnumerateFileSystemInfos().ToList());
        }

        private sealed class FailingStream : MemoryStream
        {
            public override int Read(byte[] buffer, int offset, int count) => throw new IOException("connection lost");

            public override int Read(Span<byte> buffer) => throw new IOException("connection lost");
        }
    }
}
