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

using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        public ReleaseServerTestHost(IDictionary<string, string> additionalSettings = null, bool useKestrel = false)
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
                // The background calculation of checksums would change the test data while a test runs;
                // the tests of this feature enable it explicitly.
                ["AddMissingArtifactHashes"] = "false",
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
                // The settings are applied like environment variables of a container.
                builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(settings));
            });

            if (useKestrel)
            {
                // A real HTTP server on a free port, e.g. for external clients like the NuGet client
                factory.UseKestrel(options => options.Listen(System.Net.IPAddress.Loopback, 0));
                factory.StartServer();
                var addresses = factory.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>().Addresses;
                ServerAddress = addresses.First().Replace("[::]", "127.0.0.1").Replace("0.0.0.0", "127.0.0.1").Replace("localhost", "127.0.0.1");
            }
        }

        public string RootDirectory { get; }

        /// <summary>
        /// Gets the address of the real HTTP server (only with useKestrel).
        /// </summary>
        public string ServerAddress { get; }

        public string ArtifactDirectory { get; }

        public string BackupDirectory { get; }

        /// <summary>
        /// Gets the services of the hosted server (starts the server if necessary).
        /// </summary>
        public IServiceProvider Services => factory.Services;

        public HttpClient CreateClient()
        {
            var options = new WebApplicationFactoryClientOptions { AllowAutoRedirect = false };
            if (ServerAddress != null)
            {
                options.BaseAddress = new Uri(ServerAddress);
            }

            return factory.CreateClient(options);
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
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                // Best effort cleanup of the temporary directory (files may still be locked on Windows).
            }
        }
    }
}
