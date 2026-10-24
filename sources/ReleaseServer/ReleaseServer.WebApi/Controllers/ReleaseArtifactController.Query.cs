//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Query.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// A secured ApiController that provides several endpoints for managing release artifacts (upload / download
    /// artifacts, get several information about the stored artifacts)
    /// </summary>
    public partial class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Public methods ----------

        /// <summary>
        /// Lists the releases of a product.
        /// </summary>
        /// <remarks>
        /// Returns the releases of a product with version, release date, supported platforms and change log.
        /// The releases can be filtered by platform and version, sorted by version and paged.
        ///
        /// A platform consists of an operating system and an architecture, e.g. "windows/x64" or "linux/arm64".
        /// To download a release, use the version and one of its platforms:
        /// `GET /artifacts/{product}/{os}/{architecture}/{version}`.
        ///
        /// Examples:
        ///
        ///     GET /artifacts/example-product/info
        ///     GET /artifacts/example-product/info?operatingSystems=windows&amp;architectures=x64,any
        ///     GET /artifacts/example-product/info?sortOrder=descending&amp;limit=1   (latest release)
        ///     GET /artifacts/example-product/info?version=1.2.0                     (one release)
        ///
        /// Pre-releases have a version suffix (e.g. 2.0.0-beta) and "isPreviewRelease": true. A release without
        /// suffix is newer than its pre-releases.
        ///
        /// "artifacts" lists the downloadable file of every platform with file name, size in bytes, SHA-256
        /// checksum (lower case hex) and download URL. The checksum of files uploaded before version 1.1 is
        /// calculated by the server in the background and is null until then.
        /// </remarks>
        /// <param name="product">The identifier of the product (case-sensitive), as listed by GET /artifacts.</param>
        /// <param name="architectures">Only releases for these architectures (comma separated or repeated parameter), e.g. "x64,arm64". Default: all.</param>
        /// <param name="operatingSystems">Only releases for these operating systems (comma separated or repeated parameter), e.g. "windows,linux". Default: all.</param>
        /// <param name="sortOrder">Order of the releases by version: "descending" (newest first, default) or "ascending".</param>
        /// <param name="limit">Paging: maximum number of releases in the response (default 50).</param>
        /// <param name="offset">Paging: number of releases to skip (default 0). The response contains "nextOffset" if more releases exist.</param>
        /// <param name="version">Only the release with exactly this version, e.g. "1.2.0" or "2.0.0-beta".</param>
        /// <response code="200">The releases of the product.</response>
        /// <response code="404">The product does not exist or no release matches the filter.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductInformation), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [SeparatedQueryString]
        [HttpGet("{product}/info")]
        public async Task<ActionResult<ProductInformation>> GetProduct(
            [Required] string product,
            [FromQuery] List<string> architectures,
            [FromQuery] List<string> operatingSystems,
            [FromQuery] SortOrder sortOrder = SortOrder.Descending,
            [FromQuery] int limit = 50,
            [FromQuery] int offset = 0,
            [FromQuery] string version = null
            )
        {
            var deployments = (await releaseArtifactService.GetDeploymentInformations(product));

            if (!deployments.IsNullOrEmpty())
            {
                deployments = deployments
                    .Where((release) => {
                        var match = true;
                        if (architectures.Count > 0)
                        {
                            match &= architectures.Contains(release.Architecture);
                        }
                        if (operatingSystems.Count > 0)
                        {
                            match &= operatingSystems.Contains(release.Os);
                        }
                        if (version != null)
                        {
                            match &= release.Version == new ProductVersion(version);
                        }
                        return match;
                    })
                    .ToList();

                if (deployments.IsNullOrEmpty())
                {
                    return NotFoundResponseFactory.Create(
                    HttpContext,
                    "Resource not found.",
                    $"There is no release of \"{product}\" that matches the specified filter criteria.");
                }

                var releases = ReleaseInformationMapper.Map(deployments).ToList();
                releases.Sort(CompareByVersion);
                if (sortOrder == SortOrder.Descending) {
                    releases.Reverse();
                }

                return new ProductInformation(product, releases, limit, offset);
            }
            else
            {
                return NotFoundResponseFactory.Create(
                    HttpContext,
                    "Resource not found.",
                    "The specified product does not exist.");
            }
        }

        /// <summary>
        /// Lists all products.
        /// </summary>
        /// <remarks>
        /// Returns the identifiers of all products. Use an identifier with GET /artifacts/{product}/info to get
        /// its releases.
        /// </remarks>
        /// <response code="200">The identifiers of all products.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductList), 200)]
        [SeparatedQueryString]
        [HttpGet("")]
        public async Task<ActionResult<ProductList>> GetProductList()
        {
            var products = (await this.releaseArtifactService.GetProductList());
            return new ProductList(products);
        }

        #endregion
    }
}