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

using Castle.Core.Internal;

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
        /// Retrieves a list with deployment information objects of all available releases of a product that can be filtered, ordered and paginated.
        /// </summary>
        /// <param name="product">The name/identifier of the product to get.</param>
        /// <param name="architectures">A comma separated list of architectures to filter for. If this parameter is not set no filter will be applied.</param>
        /// <param name="operatingSystems">A comma separated list of operating systems to filter for. If this parameter is not set no filter will be applied.</param>
        /// <param name="sortOrder">Defines how the results will be ordered by their version numbers.</param>
        /// <param name="limit">Paging parameter: Maximum number of elements that will be returned.</param>
        /// <param name="offset">Paging parameter: Offset of the first element to be returned.</param>
        /// <param name="version">Can be used to select a specific version of the product (for e.g. obtaining the changelog of this version).</param>
        /// <response code="200">A product with the specified product name exists.</response>
        /// <response code="404">The specified product does not exist.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductInformation), 200)]
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
        /// Retrieves a list of all products stored in the server.
        /// </summary>
        /// <response code="200">List of all products.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductList), 200)]
        [SeparatedQueryString]
        [HttpGet("")]
        public async Task<ActionResult<ProductList>> GetProductList()
        {
            var products = (await this.releaseArtifactService.GetProductList());
            return new ProductList(products);
        }

        ///// <summary>
        ///// Retrieves a list of all available versions of the specified artifact.
        ///// </summary>
        ///// <param name="product">The product name of the searched artifact.</param>
        ///// <returns>A <see cref="DeploymentInformationList"/> with the available product infos. A <see cref="NotFoundObjectResult"/>, if
        ///// the specified artifact does not exist.</returns>
        ///// <response code="200">An artifact with the specified product name exists.</response>
        ///// <response code="404">The specified artifact does not exist.</response>
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(DeploymentInformationList), 200)]
        //[HttpGet("versions/{product}")]
        //public async Task<ActionResult<DeploymentInformationList>> GetProductInfos([Required] string product)
        //{
        //    var productInfos = await releaseArtifactService.GetDeploymentInformations(product);

        //    if (!productInfos.IsNullOrEmpty())
        //    {
        //        return new DeploymentInformationList(productInfos);
        //    }
        //    else
        //    {
        //        return NotFoundResponseFactory.Create(
        //            HttpContext,
        //            "Resource not found.",
        //            "The specified product does not exist.");
        //    }
        //}

        ///// <summary>
        ///// Retrieves all available platforms for a specific artifact.
        ///// </summary>
        ///// <param name="product">The product name of the artifact.</param>
        ///// <param name="version">The version of the artifact.</param>
        ///// <returns>A <see cref="PlatformsList"/> with the available platforms. A <see cref="NotFoundObjectResult"/>, if
        ///// no platforms were found or if there exists no artifact with the specified product name.</returns>
        ///// <response code="200">There are existing platforms for the specified product name.</response>
        ///// <response code="404">The artifact does not exist or there exists no platform for the specified product name.</response>
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(PlatformsList), 200)]
        //[HttpGet("platforms/{product}/{version}")]
        //public async Task<ActionResult<PlatformsList>> GetPlatforms([Required] string product, [Required]string version)
        //{
        //    var platformsList = await releaseArtifactService.GetPlatforms(product, version);

        //    if (!platformsList.IsNullOrEmpty())
        //    {

        //        return new PlatformsList(platformsList);
        //    }
        //    else
        //    {
        //        return NotFoundResponseFactory.Create(
        //            HttpContext,
        //            "Resource not found.",
        //            "The specified product does not exist.");
        //    }
        //}

        ///// <summary>
        ///// Retrieves the release information of a specific artifact.
        ///// </summary>
        ///// <param name="product">The product name of the specified artifact.</param>
        ///// <param name="os">The operating system of the specified artifact.</param>
        ///// <param name="architecture">The hardware architecture of the specified artifact.</param>
        ///// <param name="version">The version of the specified artifact.</param>
        ///// <returns>A <see cref="DeploymentInformation"/> with the release information. A <see cref="NotFoundObjectResult"/>, if
        ///// the release information for the specified artifact was not found.</returns>
        ///// <response code="200">The release information of the specified artifact exists.</response>
        ///// <response code="404">The artifact with the specified product name does not exist. Therefore the release notes do not exist.</response>
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(DeploymentInformation), 200)]
        //[HttpGet("info/{product}/{os}/{architecture}/{version}")]
        //public async Task<ActionResult<DeploymentInformation>> GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        //{
        //    var result = await releaseArtifactService.GetDeploymentInformation(product, os, architecture, version);

        //    if (result != null)
        //    {

        //        return result;
        //    }
        //    else
        //    {
        //        return NotFoundResponseFactory.Create(
        //            HttpContext,
        //            "Resource not found.",
        //            "The specified deployment does not exist.");
        //    }
        //}

        ///// <summary>
        ///// Retrieves all available versions that are fitting to a specific product name / platform (HW architecture + OS).
        ///// </summary>
        ///// <param name="product">The product name of the specified artifact.</param>
        ///// <param name="os">The operating system of the specified artifact.</param>
        ///// <param name="architecture">The hardware architecture of the specified artifact.</param>
        ///// <returns>A <see cref="ProductVersionList"/> with the available versions. A <see cref="NotFoundObjectResult"/>, if
        ///// there exists no version for the specified platform / product name.</returns>
        ///// <response code="200">There are existing versions for the specified platform and product.</response>
        ///// <response code="404">There exists no version for the specified platform / product.</response>
        //[AllowAnonymous]
        //[ProducesResponseType(typeof(ProductVersionList), 200)]
        //[HttpGet("versions/{product}/{os}/{architecture}")]
        //public async Task<ActionResult<ProductVersionList>> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        //{
        //    var productVersions = await releaseArtifactService.GetVersions(product, os, architecture);

        //    if (!productVersions.IsNullOrEmpty())
        //    {
        //        return new ProductVersionList(productVersions);
        //    }
        //    else
        //    {
        //        return NotFoundResponseFactory.Create(
        //            HttpContext,
        //            "Resource not found.",
        //            "No versions for the specified platform exist.");
        //    }
        //}

        ///// <summary>
        ///// Retrieves the latest version of a specific artifact.
        ///// </summary>
        ///// <param name="product">The product name of the specified artifact.</param>
        ///// <param name="os">The operating system of the specified artifact.</param>
        ///// <param name="architecture">The hardware architecture of the specified artifact.</param>
        ///// <returns>A <see cref="ProductVersionResponse"/> with the latest version information.A <see cref="NotFoundObjectResult"/>,
        ///// if not there is no artifact available for the specified parameters.</returns>
        ///// <response code="200">The specified product exists.</response>
        ///// <response code="404">The artifact is not available for the specified platform (OS + HW architecture)
        ///// or the artifact with the specified product name does not exist</response>
        //[AllowAnonymous]
        //[HttpGet("latest/{product}/{os}/{architecture}")]
        //[ProducesResponseType(typeof(ProductVersionResponse), 200)]
        //public async Task<ActionResult<ProductVersionResponse>> GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        //{
        //    var latestVersion = await releaseArtifactService.GetLatestVersion(product, os, architecture);

        //    if (latestVersion != null)
        //    {
        //        return new ProductVersionResponse(latestVersion);
        //    }
        //    else
        //    {
        //        return NotFoundResponseFactory.Create(
        //            HttpContext,
        //            "Resource not found.",
        //            "The specified deployment does not exist.");
        //    }
        //}

        #endregion
    }
}