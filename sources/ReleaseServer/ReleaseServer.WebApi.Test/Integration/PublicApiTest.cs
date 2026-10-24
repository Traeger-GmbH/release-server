//--------------------------------------------------------------------------------------------------
// <copyright file="PublicApiTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Pins the anonymous (public) endpoints that customers and the website use. URLs, status codes,
    /// headers and response bodies must stay backwards compatible.
    /// </summary>
    [Collection(IntegrationCollection.Name)]
    public sealed class PublicApiTest : IDisposable
    {
        private readonly ReleaseServerTestHost host;
        private readonly HttpClient client;

        public PublicApiTest()
        {
            host = new ReleaseServerTestHost();
            LegacyStore.Create(host.ArtifactDirectory);
            client = host.CreateClient();
        }

        public void Dispose()
        {
            client.Dispose();
            host.Dispose();
        }

        [Fact]
        public async Task GetProductList_ReturnsAllProductDirectories()
        {
            var response = await client.GetAsync("/artifacts/");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
            Golden.AssertJson("products", await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetProductList_SkipsDirectorySkeletonsWithoutReleases()
        {
            // Deliberate change compared to the 3.1 server: product directories without any release (e.g. the
            // skeleton left behind by deleting all releases, like "pc-s7-link-demo2" in production) were listed
            // as products, although /info returned 404. They are no longer listed; /info is unchanged (404).
            System.IO.Directory.CreateDirectory(System.IO.Path.Combine(host.ArtifactDirectory, "skeleton", "windows", "x64"));

            var products = await client.GetStringAsync("/artifacts/");
            var info = await client.GetAsync("/artifacts/pc-s7-link-demo2/info");

            Assert.DoesNotContain("pc-s7-link-demo2", products);
            Assert.DoesNotContain("skeleton", products);
            Assert.Equal(HttpStatusCode.NotFound, info.StatusCode);
            Golden.AssertJson("info-empty-product", await info.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetProductList_WithoutTrailingSlash_ReturnsProducts()
        {
            var response = await client.GetAsync("/artifacts");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Golden.AssertJson("products", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("info-alpha", "/artifacts/alpha/info")]
        [InlineData("info-alpha-ascending", "/artifacts/alpha/info?sortOrder=ascending")]
        [InlineData("info-alpha-descending", "/artifacts/alpha/info?sortOrder=descending")]
        [InlineData("info-alpha-paged", "/artifacts/alpha/info?limit=2&offset=1")]
        [InlineData("info-alpha-limit-zero", "/artifacts/alpha/info?limit=0")]
        [InlineData("info-alpha-offset-beyond", "/artifacts/alpha/info?offset=50")]
        [InlineData("info-alpha-architecture", "/artifacts/alpha/info?architectures=x64")]
        [InlineData("info-alpha-architectures-separated", "/artifacts/alpha/info?architectures=x86,any")]
        [InlineData("info-alpha-os", "/artifacts/alpha/info?operatingSystems=linux")]
        [InlineData("info-alpha-os-repeated", "/artifacts/alpha/info?operatingSystems=linux&operatingSystems=windows")]
        [InlineData("info-alpha-version", "/artifacts/alpha/info?version=1.10-beta")]
        [InlineData("info-alpha-version-and-os", "/artifacts/alpha/info?version=1.0&operatingSystems=windows")]
        [InlineData("info-s5-upper-case", "/artifacts/s5-lan-3964R-lan-manager/info")]
        [InlineData("info-teleprof", "/artifacts/teleprof2-os-firmware/info")]
        public async Task GetProductInfo_MatchesGoldenFile(string goldenName, string url)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task GetProductInfo_RawBody_KeepsEncodingAndFormatting()
        {
            // The raw body pins the exact serialization (escaping, broken characters, date format).
            var response = await client.GetAsync("/artifacts/s5-lan-3964R-lan-manager/info");

            Golden.AssertText("info-s5-upper-case.raw.txt", await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("info-unknown-product", "/artifacts/unknown/info")]
        [InlineData("info-empty-product", "/artifacts/pc-s7-link-demo2/info")]
        [InlineData("info-no-match", "/artifacts/alpha/info?operatingSystems=macos")]
        [InlineData("info-unknown-version", "/artifacts/alpha/info?version=9.9")]
        [InlineData("info-wrong-product-case", "/artifacts/ALPHA/info")]
        public async Task GetProductInfo_NotFound_MatchesGoldenFile(string goldenName, string url)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Golden.AssertText(goldenName + ".content-type.txt", response.Content.Headers.ContentType?.ToString() ?? "<none>");
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/artifacts/alpha/windows/x86/1.0", "alpha/windows/x86/1.0", "alpha-setup.exe")]
        [InlineData("/artifacts/alpha/linux/x64/1.0", "alpha/linux/x64/1.0", "alpha.tar.gz")]
        [InlineData("/artifacts/alpha/windows/any/1.10", "alpha/windows/any/1.10", "alpha-1.10.zip")]
        [InlineData("/artifacts/alpha/windows/any/1.10-beta", "alpha/windows/any/1.10-beta", "alpha-1.10-beta.zip")]
        [InlineData("/artifacts/s5-lan-3964R-lan-manager/Windows/X86/1.36", "s5-lan-3964R-lan-manager/Windows/X86/1.36", "Setup S5 LAN.exe")]
        [InlineData("/artifacts/s5-lan-3964R-lan-manager/windows-xp/any/1.7", "s5-lan-3964R-lan-manager/windows-xp/any/1.7", "s5lan-1.7.zip")]
        [InlineData("/artifacts/teleprof2-os-firmware/teleprof/2/8.14.0.1", "teleprof2-os-firmware/teleprof/2/8.14.0.1", "firmware.bin")]
        public async Task GetSpecificArtifact_ReturnsFileWithHeaders(string url, string expectedContentKey, string fileName)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(LegacyStore.ArtifactContent(expectedContentKey), await response.Content.ReadAsByteArrayAsync());
            Golden.AssertText("download-" + fileName.Replace(' ', '_') + ".headers.txt", DescribeDownloadHeaders(response));
        }

        [Theory]
        [InlineData("/artifacts/alpha/windows/x86/1.0")]
        [InlineData("/artifacts/alpha/windows/x86/latest")]
        public async Task GetArtifact_RangeRequest_ReturnsPartialContent(string url)
        {
            var content = LegacyStore.ArtifactContent("alpha/windows/x86/1.0");
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(5, 12);

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.PartialContent, response.StatusCode);
            Assert.Equal(content.Skip(5).Take(8).ToArray(), await response.Content.ReadAsByteArrayAsync());
            Assert.Equal(content.Length, response.Content.Headers.ContentRange.Length);
        }

        [Fact]
        public async Task GetArtifact_ContentLength_IsSet()
        {
            var response = await client.GetAsync("/artifacts/alpha/windows/x86/1.0");

            Assert.Equal(LegacyStore.ArtifactContent("alpha/windows/x86/1.0").Length, response.Content.Headers.ContentLength);
        }

        [Fact]
        public async Task GetArtifact_MissingArtifactFile_ReturnsServerErrorWithoutLeakingData()
        {
            System.IO.File.Delete(System.IO.Path.Combine(host.ArtifactDirectory, "alpha", "windows", "x86", "1.0", "alpha-setup.exe"));

            var response = await client.GetAsync("/artifacts/alpha/windows/x86/1.0");

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            Assert.DoesNotContain(host.RootDirectory, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("/artifacts/alpha/windows/any/latest", "alpha/windows/any/1.10")]
        [InlineData("/artifacts/alpha/windows/x86/latest", "alpha/windows/x86/1.0")]
        [InlineData("/artifacts/s5-lan-3964R-lan-manager/Windows/X86/latest", "s5-lan-3964R-lan-manager/Windows/X86/1.36")]
        [InlineData("/artifacts/teleprof2-os-firmware/teleprof/2/latest", "teleprof2-os-firmware/teleprof/2/8.14.0.1")]
        public async Task GetLatestArtifact_ReturnsHighestVersion(string url, string expectedContentKey)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(LegacyStore.ArtifactContent(expectedContentKey), await response.Content.ReadAsByteArrayAsync());
        }

        [Fact]
        public async Task GetLatestArtifact_PreReleaseOrder_IsOrdinal()
        {
            // Pre-releases of the same version are ordered ordinal by their suffix ("rc" > "beta") and are
            // lower than the release itself. Removing the release must therefore yield "1.10-rc".
            System.IO.Directory.Delete(System.IO.Path.Combine(host.ArtifactDirectory, "alpha", "windows", "any", "1.10"), true);

            var response = await client.GetAsync("/artifacts/alpha/windows/any/latest");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(LegacyStore.ArtifactContent("alpha/windows/any/1.10-rc"), await response.Content.ReadAsByteArrayAsync());
        }

        [Theory]
        [InlineData("download-unknown-version", "/artifacts/alpha/windows/x86/9.9")]
        [InlineData("download-unknown-platform", "/artifacts/alpha/macos/arm64/1.0")]
        [InlineData("download-unknown-product", "/artifacts/unknown/windows/x86/1.0")]
        [InlineData("latest-unknown-platform", "/artifacts/alpha/macos/arm64/latest")]
        [InlineData("latest-unknown-product", "/artifacts/unknown/windows/x86/latest")]
        public async Task GetArtifact_NotFound_MatchesGoldenFile(string goldenName, string url)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Golden.AssertText(goldenName + ".content-type.txt", response.Content.Headers.ContentType?.ToString() ?? "<none>");
            Golden.AssertJson(goldenName, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("catch-all-deep", "/artifacts/a/b/c/d/e/f")]
        [InlineData("catch-all-two-segments", "/artifacts/alpha/windows")]
        public async Task UnknownArtifactRoute_ReturnsNotFound(string goldenName, string url)
        {
            var response = await client.GetAsync(url);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Golden.AssertText(goldenName + ".txt", DescribeBody(response, await response.Content.ReadAsStringAsync()));
        }

        [Fact]
        public async Task UnknownRootRoute_ReturnsNotFound()
        {
            var response = await client.GetAsync("/does-not-exist");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task SwaggerDocument_ListsPublicApiSurface()
        {
            var response = await client.GetAsync("/swagger/v1/swagger.json");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var document = JObject.Parse(await response.Content.ReadAsStringAsync());
            var operations = ((JObject)document["paths"]).Properties()
                .SelectMany(path => ((JObject)path.Value).Properties().Select(method => method.Name.ToUpperInvariant() + " " + path.Name))
                .OrderBy(operation => operation, StringComparer.Ordinal);

            Golden.AssertText("swagger-operations.txt", string.Join("\n", operations) + "\n");
        }

        [Fact]
        public async Task SwaggerUi_IsPubliclyAvailable()
        {
            var response = await client.GetAsync("/swagger/index.html");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Cors_AllowsAnyOriginForPublicGet()
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/artifacts/alpha/info");
            request.Headers.Add("Origin", "https://www.example.com");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal("https://www.example.com", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        }

        private static string DescribeDownloadHeaders(HttpResponseMessage response)
        {
            var contentDisposition = response.Content.Headers.ContentDisposition?.ToString()
                ?? (response.Headers.TryGetValues("Content-Disposition", out var values) ? string.Join(", ", values) : "<none>");

            return $"Content-Type: {response.Content.Headers.ContentType}\n" +
                   $"Content-Disposition: {contentDisposition}\n";
        }

        private static string DescribeBody(HttpResponseMessage response, string body)
        {
            return $"Content-Type: {response.Content.Headers.ContentType?.ToString() ?? "<none>"}\n" +
                   $"Body: {body}\n";
        }
    }
}
