//--------------------------------------------------------------------------------------------------
// <copyright file="HeadRequestTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// HEAD requests (e.g. of link checkers) get the same status and headers as GET, without body. Tested
    /// against a real HTTP server, because only Kestrel suppresses the body like in production.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class HeadRequestTest : IDisposable
    {
        // Headers that differ between two requests or depend on the transfer of the body
        private static readonly string[] VolatileHeaders = { "Date", "Transfer-Encoding" };

        private readonly ReleaseServerTestHost host;
        private readonly HttpClient client;

        public HeadRequestTest()
        {
            host = new ReleaseServerTestHost(useKestrel: true);
            LegacyStore.Create(host.ArtifactDirectory);
            client = host.CreateClient();

            // A release with checksum (ETag, Repr-Digest) and a NuGet package for the feed
            using var authenticated = host.CreateAuthenticatedClient();
            var package = NuGetFeedTest.CreatePackage("Opc.UaFx.Client", "2.57.1");
            var payload = Zip.CreateArtifactPayload("opc.uafx.client.2.57.1.2026-10-23-2.57.1.0.nupkg", package, LegacyStore.ReleaseNotesFull);
            var response = authenticated.PutAsync("/artifacts/Opc.UaFx.Client.nupkg/any/any/2.57.1",
                Zip.CreateFormContent("artifact", payload, "payload.zip")).GetAwaiter().GetResult();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        public void Dispose()
        {
            client.Dispose();
            host.Dispose();
        }

        public static IEnumerable<object[]> PublicUrls()
        {
            return new[]
            {
                // Release API
                "/artifacts/",
                "/artifacts",
                "/artifacts/alpha/info",
                "/artifacts/alpha/info?sortOrder=ascending&limit=2",
                "/artifacts/alpha/windows/x86/1.0",
                "/artifacts/alpha/windows/any/latest",
                "/artifacts/s5-lan-3964R-lan-manager/Windows/X86/1.36",
                "/artifacts/Opc.UaFx.Client.nupkg/any/any/2.57.1",
                "/artifacts/Opc.UaFx.Client.nupkg/any/any/latest",
                // Not found (problem details) and the catch-all route
                "/artifacts/unknown/info",
                "/artifacts/alpha/windows/x86/9.9",
                "/artifacts/alpha/macos/arm64/latest",
                "/artifacts/a/b/c/d/e/f",
                // NuGet feed
                "/nuget/v3/index.json",
                "/nuget/v3/flatcontainer/opc.uafx.client/index.json",
                "/nuget/v3/flatcontainer/opc.uafx.client/2.57.1/opc.uafx.client.2.57.1.nupkg",
                "/nuget/v3/flatcontainer/opc.uafx.client/2.57.1/opc.uafx.client.nuspec",
                "/nuget/v3/registration/opc.uafx.client/index.json",
                "/nuget/v3/registration/opc.uafx.client/2.57.1.json",
                "/nuget/v3/search?q=opc",
                "/nuget/v3/flatcontainer/unknown/index.json",
            }.Select(url => new object[] { url });
        }

        [Theory]
        [MemberData(nameof(PublicUrls))]
        public async Task Head_ReturnsStatusAndHeadersOfGet_WithoutBody(string url)
        {
            var get = await client.GetAsync(url);
            var getBody = await get.Content.ReadAsByteArrayAsync();
            var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
            var headBody = await head.Content.ReadAsByteArrayAsync();

            Assert.Equal(get.StatusCode, head.StatusCode);
            Assert.Equal(Headers(get), Headers(head));
            Assert.Empty(headBody);

            // Content-Length (if sent) describes the GET body
            if (get.Content.Headers.ContentLength.HasValue)
            {
                Assert.Equal(getBody.Length, head.Content.Headers.ContentLength);
            }
        }

        [Fact]
        public async Task Head_OfDownload_ContainsAllDownloadHeaders()
        {
            var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, "/artifacts/Opc.UaFx.Client.nupkg/any/any/2.57.1"));

            Assert.Equal(HttpStatusCode.OK, head.StatusCode);
            Assert.Equal("application/octet-stream", head.Content.Headers.ContentType.ToString());
            Assert.Equal("attachment; filename=opc.uafx.client.2.57.1.2026-10-23-2.57.1.0.nupkg", head.Content.Headers.ContentDisposition.ToString());
            Assert.NotNull(head.Content.Headers.ContentLength);
            Assert.NotNull(head.Headers.ETag);
            Assert.True(head.Headers.Contains("Repr-Digest"));
            Assert.Equal("bytes", head.Headers.AcceptRanges.Single());
        }

        [Theory]
        [InlineData("/artifacts/statistics")]
        [InlineData("/artifacts/backup")]
        public async Task Head_OfAuthenticatedEndpoints_IsNotSupported(string url)
        {
            // Only the public endpoints answer HEAD; the authenticated ones keep their previous behavior
            var head = await client.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));

            Assert.Equal(HttpStatusCode.NotFound, head.StatusCode);
        }

        private static SortedDictionary<string, string> Headers(HttpResponseMessage response)
        {
            var headers = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, values) in response.Headers.Concat(response.Content.Headers))
            {
                if (!VolatileHeaders.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    headers[name] = string.Join(", ", values);
                }
            }

            // The trace id in problem details bodies differs per request, but the length of the body as well
            if (response.Content.Headers.ContentType?.MediaType == "application/problem+json")
            {
                headers.Remove("Content-Length");
            }

            return headers;
        }
    }
}
