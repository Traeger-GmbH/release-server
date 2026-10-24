//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseServerTestHost.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// All integration tests share this collection, so they never run in parallel. The tests start
    /// complete server instances, each one with its own artifact and backup directory.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public class IntegrationCollection
    {
        public const string Name = "Integration";
    }

    /// <summary>
    /// Hosts a complete release server (the real Startup and middleware pipeline) on top of a private
    /// temporary directory. The artifact directory can be prefilled before the first request.
    /// </summary>
    public sealed class ReleaseServerTestHost : IDisposable
    {
        public const string Username = "integration-user";
        public const string Password = "integration-password";

        private readonly WebApplicationFactory<Startup> factory;

        public ReleaseServerTestHost(IDictionary<string, string> additionalSettings = null)
        {
            RootDirectory = Path.Combine(Path.GetTempPath(), "release-server-tests", Guid.NewGuid().ToString("N"));
            ArtifactDirectory = Path.Combine(RootDirectory, "artifacts");
            BackupDirectory = Path.Combine(RootDirectory, "backups");
            Directory.CreateDirectory(ArtifactDirectory);
            Directory.CreateDirectory(BackupDirectory);

            var settings = new Dictionary<string, string>
            {
                ["CREDENTIALS_USERNAME"] = Username,
                ["CREDENTIALS_PASSWORD"] = Password,
                ["ArtifactRootDirectory"] = ArtifactDirectory,
                ["BackupRootDirectory"] = BackupDirectory,
            };

            if (additionalSettings != null)
            {
                foreach (var (key, value) in additionalSettings)
                {
                    settings[key] = value;
                }
            }

            factory = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(settings));
                builder.ConfigureTestServices(services =>
                {
                    // Point the service to the private test directories, independent of how the
                    // application reads its configuration.
                    services.AddSingleton<IReleaseArtifactService>(serviceProvider =>
                        new FsReleaseArtifactService(
                            new FsReleaseArtifactRepository(
                                serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactRepository>>(),
                                new DirectoryInfo(ArtifactDirectory),
                                new DirectoryInfo(BackupDirectory)),
                            serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactService>>()));
                });
            });
        }

        public string RootDirectory { get; }

        public string ArtifactDirectory { get; }

        public string BackupDirectory { get; }

        public HttpClient CreateClient()
        {
            return factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        public HttpClient CreateAuthenticatedClient()
        {
            var client = CreateClient();
            client.DefaultRequestHeaders.Authorization = CreateBasicAuthHeader(Username, Password);
            return client;
        }

        public static AuthenticationHeaderValue CreateBasicAuthHeader(string username, string password)
        {
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(username + ":" + password));
            return new AuthenticationHeaderValue("Basic", token);
        }

        /// <summary>
        /// Returns all files below the artifact directory as relative path (with '/' separators) and
        /// content, ordered by path. Empty directories are included with a trailing '/' and no content
        /// (like the entries of a ZIP archive). Used to compare complete data sets byte by byte.
        /// </summary>
        public static SortedDictionary<string, byte[]> ReadTree(string directory)
        {
            var result = new SortedDictionary<string, byte[]>(StringComparer.Ordinal);

            foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
            {
                result[RelativePath(directory, file)] = File.ReadAllBytes(file);
            }

            foreach (var subDirectory in Directory.EnumerateDirectories(directory, "*", SearchOption.AllDirectories))
            {
                if (!Directory.EnumerateFileSystemEntries(subDirectory).Any())
                {
                    result[RelativePath(directory, subDirectory) + "/"] = Array.Empty<byte>();
                }
            }

            return result;
        }

        private static string RelativePath(string root, string path)
        {
            return Path.GetRelativePath(root, path).Replace(Path.DirectorySeparatorChar, '/');
        }

        public static void AssertSameTree(SortedDictionary<string, byte[]> expected, SortedDictionary<string, byte[]> actual)
        {
            Assert.Equal(expected.Keys.ToList(), actual.Keys.ToList());

            foreach (var (path, content) in expected)
            {
                Assert.True(content.SequenceEqual(actual[path]), $"Content of '{path}' differs.");
            }
        }

        public void Dispose()
        {
            factory.Dispose();

            try
            {
                Directory.Delete(RootDirectory, true);
            }
            catch (IOException)
            {
                // Best effort cleanup of the temporary directory.
            }
        }
    }
}
