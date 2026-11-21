//--------------------------------------------------------------------------------------------------
// <copyright file="RobotsHeaderTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Every response carries "X-Robots-Tag: noindex", so search engines do not index downloads, API
    /// responses, the NuGet feed or the Swagger UI. Nothing else of the responses changes.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class RobotsHeaderTest : IDisposable
    {
        private readonly ReleaseServerTestHost host;
        private readonly HttpClient client;
        private readonly HttpResponseMessage uploadResponse;

        public RobotsHeaderTest()
        {
            host = new ReleaseServerTestHost(useKestrel: true);
            LegacyStore.Create(host.ArtifactDirectory);
            client = host.CreateClient();

            // A NuGet package, so the feed has content
            using var authenticated = host.CreateAuthenticatedClient();
            var package = NuGetFeedTest.CreatePackage("Opc.UaFx.Client", "2.57.1");
            var payload = Zip.CreateArtifactPayload("opc.uafx.client.2.57.1.2026-10-23-2.57.1.0.nupkg", package, LegacyStore.ReleaseNotesFull);
            uploadResponse = authenticated.PutAsync("/artifacts/Opc.UaFx.Client.nupkg/any/any/2.57.1",
                Zip.CreateFormContent("artifact", payload, "payload.zip")).GetAwaiter().GetResult();
            Assert.Equal(HttpStatusCode.OK, uploadResponse.StatusCode);
        }

        public void Dispose()
        {
            uploadResponse.Dispose();
            client.Dispose();
            host.Dispose();
        }

        public static IEnumerable<object[]> Requests()
        {
            return new[]
            {
                // Release API: products, info, downloads
                ("GET", "/artifacts/", HttpStatusCode.OK),
                ("GET", "/artifacts/alpha/info", HttpStatusCode.OK),
                ("GET", "/artifacts/alpha/windows/x86/1.0", HttpStatusCode.OK),
                ("GET", "/artifacts/alpha/windows/any/latest", HttpStatusCode.OK),
                ("HEAD", "/artifacts/alpha/windows/x86/1.0", HttpStatusCode.OK),
                ("HEAD", "/artifacts/", HttpStatusCode.OK),
                // Not found (problem details), unknown routes and authenticated endpoints without login
                ("GET", "/artifacts/unknown/info", HttpStatusCode.NotFound),
                ("GET", "/artifacts/alpha/windows/x86/9.9", HttpStatusCode.NotFound),
                ("GET", "/", HttpStatusCode.NotFound),
                ("GET", "/robots.txt", HttpStatusCode.NotFound),
                ("GET", "/artifacts/statistics", HttpStatusCode.Unauthorized),
                // NuGet feed
                ("GET", "/nuget/v3/index.json", HttpStatusCode.OK),
                ("GET", "/nuget/v3/flatcontainer/opc.uafx.client/2.57.1/opc.uafx.client.2.57.1.nupkg", HttpStatusCode.OK),
                ("GET", "/nuget/v3/search?q=opc", HttpStatusCode.OK),
                // Swagger
                ("GET", "/swagger/index.html", HttpStatusCode.OK),
                ("GET", "/swagger/v1/swagger.json", HttpStatusCode.OK),
            }.Select(request => new object[] { request.Item1, request.Item2, request.Item3 });
        }

        [Theory]
        [MemberData(nameof(Requests))]
        public async Task Response_HasNoIndexHeader(string method, string url, HttpStatusCode expectedStatus)
        {
            var response = await client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url));

            Assert.Equal(expectedStatus, response.StatusCode);
            Assert.Equal("noindex", Assert.Single(response.Headers.GetValues("X-Robots-Tag")));
        }

        [Fact]
        public async Task AuthenticatedResponses_HaveNoIndexHeader()
        {
            using var authenticated = host.CreateAuthenticatedClient();

            var statistics = await authenticated.GetAsync("/artifacts/statistics");

            Assert.Equal(HttpStatusCode.OK, statistics.StatusCode);
            Assert.Equal("noindex", Assert.Single(statistics.Headers.GetValues("X-Robots-Tag")));
            Assert.Equal("noindex", Assert.Single(uploadResponse.Headers.GetValues("X-Robots-Tag")));
        }

        [Fact]
        public async Task Download_KeepsCorsAndDownloadHeaders()
        {
            // The downloads page of the website reads the server across origins; the header must not change that
            using var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/alpha/windows/x86/1.0");
            request.Headers.Add("Origin", "https://www.example.org");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("https://www.example.org", Assert.Single(response.Headers.GetValues("Access-Control-Allow-Origin")));
            Assert.Equal("attachment; filename=alpha-setup.exe", response.Content.Headers.ContentDisposition.ToString());
            Assert.Equal("noindex", Assert.Single(response.Headers.GetValues("X-Robots-Tag")));
        }
    }
}
