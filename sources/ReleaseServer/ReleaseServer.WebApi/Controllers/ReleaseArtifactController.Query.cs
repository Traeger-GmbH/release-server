//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Query.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
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
        /// Retrieves a list of all available versions of the specified artifact.
        /// </summary>
        /// <param name="product">The product name of the searched artifact.</param>
        /// <returns>A <see cref="ProductInformationList"/> with the available product infos. A <see cref="NotFoundObjectResult"/>, if
        /// the specified artifact does not exist.</returns>
        /// <response code="200">An artifact with the specified product name exists.</response>
        /// <response code="404">The specified artifact does not exist.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductInformationList), 200)]
        [HttpGet("versions/{product}")]
        public async Task<ActionResult<ProductInformationList>> GetProductInfos([Required] string product)
        {
            var productInfos = await releaseArtifactService.GetProductInfos(product);

            if (productInfos.IsNullOrEmpty()) 
                return NotFound("The specified artifact was not found!");
            
            return new ProductInformationList(productInfos);
        }

        /// <summary>
        /// Retrieves all available platforms for a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the artifact.</param>
        /// <param name="version">The version of the artifact.</param>
        /// <returns>A <see cref="PlatformsList"/> with the available platforms. A <see cref="NotFoundObjectResult"/>, if
        /// no platforms were found or if there exists no artifact with the specified product name.</returns>
        /// <response code="200">There are existing platforms for the specified product name.</response>
        /// <response code="404">The artifact does not exist or there exists no platform for the specified product name.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlatformsList), 200)]
        [HttpGet("platforms/{product}/{version}")]
        public async Task<ActionResult<PlatformsList>> GetPlatforms([Required] string product, [Required]string version)
        {
            var platformsList = await releaseArtifactService.GetPlatforms(product, version);

            if (platformsList.IsNullOrEmpty()) 
                return NotFound("The specified artifact was not found or there exists no platform for the specified product name!");
            
            
            return new PlatformsList(platformsList);
        }
        
        /// <summary>
        /// Retrieves the release information of a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <param name="version">The version of the specified artifact.</param>
        /// <returns>A <see cref="ReleaseInformation"/> with the release information. A <see cref="NotFoundObjectResult"/>, if
        /// the release information for the specified artifact was not found.</returns>
        /// <response code="200">The release information of the specified artifact exists.</response>
        /// <response code="404">The artifact with the specified product name does not exist. Therefore the release notes do not exist.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReleaseInformation), 200)]
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public async Task<ActionResult<ReleaseInformation>> GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var releaseInfo = await releaseArtifactService.GetReleaseInfo(product, os, architecture, version);

            if (releaseInfo == null)
                return NotFound("The Release information does not exist (the specified artifact was not found)!");

            return releaseInfo;
        }
        
        /// <summary>
        /// Retrieves all available versions that are fitting to a specific product name / platform (HW architecture + OS).
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <returns>A <see cref="ProductVersionList"/> with the available versions. A <see cref="NotFoundObjectResult"/>, if
        /// there exists no version for the specified platform / product name.</returns>
        /// <response code="200">There are existing versions for the specified platform and product.</response>
        /// <response code="404">There exists no version for the specified platform / product.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductVersionList), 200)]
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public async Task<ActionResult<ProductVersionList>> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            var productVersions = await releaseArtifactService.GetVersions(product, os, architecture);
            
            if (productVersions.IsNullOrEmpty()) 
                return NotFound("No versions for the specified platform / product name found!");

            return new ProductVersionList(productVersions);
        }
        
        /// <summary>
        /// Retrieves the latest version of a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <returns>A <see cref="ProductVersionResponse"/> with the latest version information.A <see cref="NotFoundObjectResult"/>,
        /// if not there is no artifact available for the specified parameters.</returns>
        /// <response code="200">The specified product exists.</response>
        /// <response code="404">The artifact is not available for the specified platform (OS + HW architecture)
        /// or the artifact with the specified product name does not exist</response>
        [AllowAnonymous]
        [HttpGet("latest/{product}/{os}/{architecture}")]
        [ProducesResponseType(typeof(ProductVersionResponse), 200)]
        public async Task<ActionResult<ProductVersionResponse>> GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            var latestVersion = await releaseArtifactService.GetLatestVersion(product, os, architecture);
            
           if (latestVersion == null)
               return NotFound("The specified artifact was not found!");
            
            return new ProductVersionResponse(latestVersion);
        }
       
        #endregion
    }
}