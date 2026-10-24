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

        #endregion
    }
}