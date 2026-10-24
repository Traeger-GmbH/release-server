//--------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Tests the Basic authentication of the write endpoints with different credentials.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class AuthenticationTest
    {
        [Theory]
        [InlineData("pass:word")]
        [InlineData("a:b:c")]
        [InlineData(":leading-colon")]
        [InlineData("p&w%d#x")]
        [InlineData("%41%3A&#=+/?")]
        [InlineData("Ünïcödé-ß-€")]
        [InlineData(" spaces inside ")]
        public async Task SpecialCharactersInPassword_AreAccepted(string password)
        {
            using var host = new ReleaseServerTestHost(new Dictionary<string, string> { ["CREDENTIALS_PASSWORD"] = password });
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = ReleaseServerTestHost.CreateBasicAuthHeader(ReleaseServerTestHost.Username, password);
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("pass:word", "pass")]
        [InlineData("pass:word", "pass:wor")]
        [InlineData("pass:word", "pass:word:")]
        [InlineData("p&w%d#x", "p&w%d#")]
        public async Task SimilarPassword_IsRejected(string configuredPassword, string sentPassword)
        {
            using var host = new ReleaseServerTestHost(new Dictionary<string, string> { ["CREDENTIALS_PASSWORD"] = configuredPassword });
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = ReleaseServerTestHost.CreateBasicAuthHeader(ReleaseServerTestHost.Username, sentPassword);
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task WrongUsername_IsRejected()
        {
            using var host = new ReleaseServerTestHost();
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = ReleaseServerTestHost.CreateBasicAuthHeader("someone-else", ReleaseServerTestHost.Password);
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Theory]
        [InlineData("basic")]
        [InlineData("BASIC")]
        public async Task SchemeName_IsCaseInsensitive(string scheme)
        {
            using var host = new ReleaseServerTestHost();
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, Base64($"{ReleaseServerTestHost.Username}:{ReleaseServerTestHost.Password}"));
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task OtherScheme_WithValidCredentials_IsRejected()
        {
            using var host = new ReleaseServerTestHost();
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Base64($"{ReleaseServerTestHost.Username}:{ReleaseServerTestHost.Password}"));
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task MissingConfiguredCredentials_DeniesAccess()
        {
            using var host = new ReleaseServerTestHost(new Dictionary<string, string>
            {
                ["CREDENTIALS_USERNAME"] = "",
                ["CREDENTIALS_PASSWORD"] = "",
            });
            using var client = host.CreateClient();

            var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/statistics");
            request.Headers.Authorization = ReleaseServerTestHost.CreateBasicAuthHeader("", "");
            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static string Base64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
    }
}
