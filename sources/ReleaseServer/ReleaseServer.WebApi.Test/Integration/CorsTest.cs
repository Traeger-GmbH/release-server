//--------------------------------------------------------------------------------------------------
// <copyright file="CorsTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Public endpoints can be read by any website; authenticated endpoints only by configured origins.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class CorsTest
    {
        private const string AllowedOrigin = "https://admin.example.com";
        private const string OtherOrigin = "https://www.example.org";

        public static IEnumerable<object[]> PublicRequests()
        {
            yield return new object[] { "/artifacts/" };
            yield return new object[] { "/artifacts/alpha/info" };
            yield return new object[] { "/artifacts/alpha/windows/x86/1.0" };
            yield return new object[] { "/artifacts/alpha/windows/x86/latest" };
        }

        public static IEnumerable<object[]> AuthenticatedRequests()
        {
            yield return new object[] { "PUT", "/artifacts" };
            yield return new object[] { "PUT", "/artifacts/alpha/windows/x86/1.0" };
            yield return new object[] { "DELETE", "/artifacts/alpha/windows/x86/1.0" };
            yield return new object[] { "DELETE", "/artifacts/alpha" };
            yield return new object[] { "PUT", "/artifacts/restore" };
            yield return new object[] { "GET", "/artifacts/backup" };
            yield return new object[] { "GET", "/artifacts/statistics" };
        }

        [Theory]
        [MemberData(nameof(PublicRequests))]
        public async Task PublicGet_IsAllowedForAnyOrigin(string url)
        {
            using var host = CreateHost();
            using var client = host.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Origin", OtherOrigin);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(OtherOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        }

        [Fact]
        public async Task PublicGet_PreflightIsAllowedForAnyOrigin()
        {
            using var host = CreateHost();
            using var client = host.CreateClient();

            var response = await client.SendAsync(Preflight("GET", "/artifacts/alpha/info", OtherOrigin));

            Assert.Equal(OtherOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        }

        [Theory]
        [MemberData(nameof(AuthenticatedRequests))]
        public async Task AuthenticatedEndpoint_PreflightFromOtherOrigin_IsNotAllowed(string method, string url)
        {
            using var host = CreateHost();
            using var client = host.CreateClient();

            var response = await client.SendAsync(Preflight(method, url, OtherOrigin));

            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        [Theory]
        [MemberData(nameof(AuthenticatedRequests))]
        public async Task AuthenticatedEndpoint_PreflightFromConfiguredOrigin_IsAllowed(string method, string url)
        {
            using var host = CreateHost();
            using var client = host.CreateClient();

            var response = await client.SendAsync(Preflight(method, url, AllowedOrigin));

            Assert.Equal(AllowedOrigin, response.Headers.GetValues("Access-Control-Allow-Origin").Single());
            Assert.Contains(method, response.Headers.GetValues("Access-Control-Allow-Methods").Single());
            Assert.Contains("authorization", response.Headers.GetValues("Access-Control-Allow-Headers").Single().ToLowerInvariant());
        }

        [Fact]
        public async Task AuthenticatedEndpoint_WithoutConfiguredOrigins_AllowsNoOrigin()
        {
            using var host = new ReleaseServerTestHost();
            using var client = host.CreateClient();

            var response = await client.SendAsync(Preflight("PUT", "/artifacts", AllowedOrigin));

            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

        private static ReleaseServerTestHost CreateHost()
        {
            var host = new ReleaseServerTestHost(new Dictionary<string, string> { ["Cors:WriteOrigins:0"] = AllowedOrigin });
            LegacyStore.Create(host.ArtifactDirectory);
            return host;
        }

        private static HttpRequestMessage Preflight(string method, string url, string origin)
        {
            var request = new HttpRequestMessage(HttpMethod.Options, url);
            request.Headers.Add("Origin", origin);
            request.Headers.Add("Access-Control-Request-Method", method);
            request.Headers.Add("Access-Control-Request-Headers", "authorization");
            return request;
        }
    }
}
