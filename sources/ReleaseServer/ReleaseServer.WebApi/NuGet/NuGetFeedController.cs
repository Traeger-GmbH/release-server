//--------------------------------------------------------------------------------------------------
// <copyright file="NuGetFeedController.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NuGet.Versioning;

namespace ReleaseServer.WebApi.NuGet
{
    /// <summary>
    /// Read-only NuGet v3 feed of all NuGet packages stored as artifacts. Supported resources (see
    /// https://learn.microsoft.com/nuget/api/overview):
    /// - PackageBaseAddress (flat container): versions, .nupkg and .nuspec; used by restore and install
    /// - RegistrationsBaseUrl: package metadata and dependencies; used by Visual Studio and "dotnet add package"
    /// - SearchQueryService: used by the package manager of Visual Studio and "dotnet package search"
    /// Publishing via the NuGet protocol is not supported; packages are uploaded like all other artifacts.
    /// </summary>
    [AllowAnonymous]
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("nuget/v3")]
    public class NuGetFeedController : ControllerBase
    {
        private const string JsonContentType = "application/json";

        private readonly NuGetPackageIndex packageIndex;
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetFeedController"/> class.
        /// </summary>
        public NuGetFeedController(NuGetPackageIndex packageIndex, IConfiguration configuration)
        {
            this.packageIndex = packageIndex;
            this.configuration = configuration;
        }

        /// <summary>
        /// The service index of the feed (the URL configured as package source).
        /// </summary>
        [HttpGet("index.json")]
        [HttpHead("index.json")]
        public IActionResult GetServiceIndex()
        {
            var resources = new JArray();
            void Add(string id, string type, string comment) =>
                resources.Add(new JObject { ["@id"] = id, ["@type"] = type, ["comment"] = comment });

            foreach (var type in new[] { "PackageBaseAddress/3.0.0" })
                Add(FeedUrl("flatcontainer/"), type, "Base URL of the package content (.nupkg, .nuspec)");
            foreach (var type in new[] { "RegistrationsBaseUrl", "RegistrationsBaseUrl/3.0.0-rc", "RegistrationsBaseUrl/3.0.0-beta", "RegistrationsBaseUrl/3.4.0", "RegistrationsBaseUrl/3.6.0", "RegistrationsBaseUrl/Versioned" })
                Add(FeedUrl("registration/"), type, "Base URL of the package metadata");
            foreach (var type in new[] { "SearchQueryService", "SearchQueryService/3.0.0-rc", "SearchQueryService/3.0.0-beta", "SearchQueryService/3.5.0" })
                Add(FeedUrl("search"), type, "Query endpoint of the package search");

            return Json(new JObject { ["version"] = "3.0.0", ["resources"] = resources });
        }

        /// <summary>
        /// All versions of a package (PackageBaseAddress).
        /// </summary>
        [HttpGet("flatcontainer/{id}/index.json")]
        [HttpHead("flatcontainer/{id}/index.json")]
        public async Task<IActionResult> GetVersions(string id)
        {
            var versions = (await FindPackages(id)).Select(package => Normalized(package.Version)).ToList();
            return versions.Count == 0 ? NotFound() : Json(new JObject { ["versions"] = new JArray(versions) });
        }

        /// <summary>
        /// The .nupkg or .nuspec of a package version (PackageBaseAddress).
        /// </summary>
        [HttpGet("flatcontainer/{id}/{version}/{fileName}")]
        [HttpHead("flatcontainer/{id}/{version}/{fileName}")]
        public async Task<IActionResult> GetPackageContent(string id, string version, string fileName)
        {
            var package = await FindPackage(id, version);
            if (package == null)
                return NotFound();

            var baseName = package.Id.ToLowerInvariant();
            if (string.Equals(fileName, $"{baseName}.{Normalized(package.Version)}.nupkg", StringComparison.OrdinalIgnoreCase))
                return new PhysicalFileResult(package.FilePath, "application/octet-stream") { EnableRangeProcessing = true };
            if (string.Equals(fileName, $"{baseName}.nuspec", StringComparison.OrdinalIgnoreCase))
                return File(package.Nuspec, "application/xml");

            return NotFound();
        }

        /// <summary>
        /// The metadata of all versions of a package (RegistrationsBaseUrl, all versions inline in one page).
        /// </summary>
        [HttpGet("registration/{id}/index.json")]
        [HttpHead("registration/{id}/index.json")]
        public async Task<IActionResult> GetRegistrationIndex(string id)
        {
            var packages = await FindPackages(id);
            if (packages.Count == 0)
                return NotFound();

            var indexUrl = RegistrationIndexUrl(packages[0].Id);
            var page = new JObject
            {
                ["@id"] = $"{indexUrl}#page/{Normalized(packages.First().Version)}/{Normalized(packages.Last().Version)}",
                ["count"] = packages.Count,
                ["items"] = new JArray(packages.Select(RegistrationLeaf)),
                ["lower"] = Normalized(packages.First().Version),
                ["upper"] = Normalized(packages.Last().Version),
            };

            return Json(new JObject { ["@id"] = indexUrl, ["count"] = 1, ["items"] = new JArray(page) });
        }

        /// <summary>
        /// The registration leaf of a package version (RegistrationsBaseUrl).
        /// </summary>
        [HttpGet("registration/{id}/{version}.json")]
        [HttpHead("registration/{id}/{version}.json")]
        public async Task<IActionResult> GetRegistrationLeaf(string id, string version)
        {
            var package = await FindPackage(id, version);
            if (package == null)
                return NotFound();

            return Json(new JObject
            {
                ["@id"] = RegistrationLeafUrl(package),
                ["catalogEntry"] = RegistrationLeafUrl(package) + "#catalogEntry",
                ["listed"] = true,
                ["packageContent"] = PackageContentUrl(package),
                ["published"] = Published(package),
                ["registration"] = RegistrationIndexUrl(package.Id),
            });
        }

        /// <summary>
        /// Searches packages by ID, title, description and tags (SearchQueryService). Prerelease versions are
        /// only included with prerelease=true, SemVer 2.0.0 versions only with semVerLevel=2.0.0.
        /// </summary>
        [HttpGet("search")]
        [HttpHead("search")]
        public async Task<IActionResult> Search([FromQuery] string q = null, [FromQuery] int skip = 0, [FromQuery] int take = 20,
            [FromQuery] bool prerelease = false, [FromQuery] string semVerLevel = null)
        {
            var includeSemVer2 = NuGetVersion.TryParse(semVerLevel ?? "", out var level) && level.Major >= 2;
            var terms = (q ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var results = (await packageIndex.GetPackages())
                .Where(package => (prerelease || !package.Version.IsPrerelease) && (includeSemVer2 || !package.Version.IsSemVer2))
                .GroupBy(package => package.Id, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.OrderBy(package => package.Version).ToList())
                .Where(versions => terms.All(term => Matches(versions.Last(), term)))
                .OrderBy(versions => versions.Last().Id, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var data = new JArray(results.Skip(Math.Max(0, skip)).Take(Math.Clamp(take, 0, 1000)).Select(versions =>
            {
                var latest = versions.Last();
                var metadata = latest.Metadata;
                return new JObject
                {
                    ["@id"] = RegistrationIndexUrl(latest.Id),
                    ["@type"] = "Package",
                    ["registration"] = RegistrationIndexUrl(latest.Id),
                    ["id"] = latest.Id,
                    ["version"] = latest.Version.ToFullString(),
                    ["description"] = metadata.GetDescription() ?? "",
                    ["summary"] = metadata.GetSummary() ?? "",
                    ["title"] = metadata.GetTitle() ?? "",
                    ["iconUrl"] = metadata.GetIconUrl() ?? "",
                    ["licenseUrl"] = metadata.GetLicenseUrl() ?? "",
                    ["projectUrl"] = metadata.GetProjectUrl() ?? "",
                    ["tags"] = new JArray(Tags(metadata)),
                    ["authors"] = new JArray(metadata.GetAuthors() ?? ""),
                    ["owners"] = new JArray(metadata.GetOwners() ?? ""),
                    ["totalDownloads"] = 0,
                    ["verified"] = false,
                    ["packageTypes"] = new JArray(new JObject { ["name"] = "Dependency" }),
                    ["versions"] = new JArray(versions.Select(version => new JObject
                    {
                        ["version"] = version.Version.ToFullString(),
                        ["downloads"] = 0,
                        ["@id"] = RegistrationLeafUrl(version),
                    })),
                };
            }));

            return Json(new JObject { ["totalHits"] = results.Count, ["data"] = data });
        }

        private JObject RegistrationLeaf(NuGetPackage package)
        {
            var metadata = package.Metadata;
            var catalogEntry = new JObject
            {
                ["@id"] = RegistrationLeafUrl(package) + "#catalogEntry",
                ["@type"] = "PackageDetails",
                ["id"] = package.Id,
                ["version"] = package.Version.ToFullString(),
                ["authors"] = metadata.GetAuthors() ?? "",
                ["description"] = metadata.GetDescription() ?? "",
                ["summary"] = metadata.GetSummary() ?? "",
                ["title"] = metadata.GetTitle() ?? "",
                ["tags"] = new JArray(Tags(metadata)),
                ["projectUrl"] = metadata.GetProjectUrl() ?? "",
                ["licenseUrl"] = metadata.GetLicenseUrl() ?? "",
                ["licenseExpression"] = metadata.GetLicenseMetadata()?.License ?? "",
                ["iconUrl"] = metadata.GetIconUrl() ?? "",
                ["requireLicenseAcceptance"] = metadata.GetRequireLicenseAcceptance(),
                ["listed"] = true,
                ["published"] = Published(package),
                ["packageContent"] = PackageContentUrl(package),
                ["dependencyGroups"] = new JArray(metadata.GetDependencyGroups().Select(group => new JObject
                {
                    ["@id"] = RegistrationLeafUrl(package) + "#dependencygroup/" + group.TargetFramework.GetShortFolderName(),
                    ["targetFramework"] = group.TargetFramework.GetShortFolderName(),
                    ["dependencies"] = new JArray(group.Packages.Select(dependency => new JObject
                    {
                        ["@id"] = RegistrationLeafUrl(package) + "#dependency/" + dependency.Id.ToLowerInvariant(),
                        ["id"] = dependency.Id,
                        ["range"] = dependency.VersionRange.ToNormalizedString(),
                        ["registration"] = RegistrationIndexUrl(dependency.Id),
                    })),
                })),
            };

            return new JObject
            {
                ["@id"] = RegistrationLeafUrl(package),
                ["catalogEntry"] = catalogEntry,
                ["packageContent"] = PackageContentUrl(package),
                ["registration"] = RegistrationIndexUrl(package.Id),
            };
        }

        private async Task<List<NuGetPackage>> FindPackages(string id)
        {
            return (await packageIndex.GetPackages())
                .Where(package => package.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
                .OrderBy(package => package.Version)
                .ToList();
        }

        private async Task<NuGetPackage> FindPackage(string id, string version)
        {
            if (!NuGetVersion.TryParse(version, out var parsed))
                return null;

            return (await FindPackages(id)).FirstOrDefault(package => package.Version == parsed);
        }

        private static bool Matches(NuGetPackage package, string term)
        {
            var metadata = package.Metadata;
            return new[] { package.Id, metadata.GetTitle(), metadata.GetDescription(), metadata.GetTags() }
                .Any(value => value != null && value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<string> Tags(global::NuGet.Packaging.NuspecReader metadata)
        {
            return (metadata.GetTags() ?? "").Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static string Normalized(NuGetVersion version)
        {
            return version.ToNormalizedString().ToLowerInvariant();
        }

        private static string Published(NuGetPackage package)
        {
            return DateTime.SpecifyKind(package.Published, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);
        }

        private string PackageContentUrl(NuGetPackage package)
        {
            var id = package.Id.ToLowerInvariant();
            var version = Normalized(package.Version);
            return FeedUrl($"flatcontainer/{id}/{version}/{id}.{version}.nupkg");
        }

        private string RegistrationIndexUrl(string id) => FeedUrl($"registration/{id.ToLowerInvariant()}/index.json");

        private string RegistrationLeafUrl(NuGetPackage package) =>
            FeedUrl($"registration/{package.Id.ToLowerInvariant()}/{Normalized(package.Version)}.json");

        /// <summary>
        /// Absolute URL of a feed resource. NuGet clients require absolute URLs. The public address is taken from
        /// the setting "PublicBaseUrl" or, behind a reverse proxy, from the forwarded protocol and host.
        /// </summary>
        private string FeedUrl(string relativePath)
        {
            var baseUrl = configuration["PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                var scheme = FirstHeaderValue("X-Forwarded-Proto") ?? Request.Scheme;
                var host = FirstHeaderValue("X-Forwarded-Host") ?? Request.Host.Value;
                baseUrl = $"{scheme}://{host}{Request.PathBase}";
            }

            return $"{baseUrl.TrimEnd('/')}/nuget/v3/{relativePath}";
        }

        private string FirstHeaderValue(string name)
        {
            var value = Request.Headers[name].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value.Split(',')[0].Trim();
        }

        private ContentResult Json(JObject value)
        {
            return Content(value.ToString(Formatting.None), JsonContentType);
        }
    }
}
