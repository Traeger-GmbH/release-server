//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Delete.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

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
        /// Deletes a release for one platform.
        /// </summary>
        /// <param name="product">The identifier of the product.</param>
        /// <param name="os">The operating system of the release.</param>
        /// <param name="architecture">The architecture of the release.</param>
        /// <param name="version">The version of the release.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the deletion was successful and <see cref="NotFoundObjectResult"/>, if not.</returns>
        /// <response code="200">The release was deleted.</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        /// <response code="404">The release does not exist.</response>
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        [EnableCors(CorsPolicies.Authenticated)]
        public async Task<IActionResult> DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var artifactFound = await releaseArtifactService.DeleteSpecificArtifactIfExists(product, os, architecture, version);
            
            if (artifactFound)
            {
                return Ok();
            }
            else
            {
                return NotFoundResponseFactory.Create(
                    this.HttpContext,
                    "Resource not found",
                    "The specified artifact does not exist.");
            }
        }
        
        /// <summary>
        /// Deletes a product with all its releases.
        /// </summary>
        /// <param name="product">The identifier of the product.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the deletion was successful and <see cref="NotFoundObjectResult"/>, if not.</returns>
        /// <response code="200">The product was deleted.</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        /// <response code="404">The product does not exist.</response>
        [HttpDelete("{product}")]
        [EnableCors(CorsPolicies.Authenticated)]
        public async Task<IActionResult> DeleteProduct ([Required] string product)
        {
            var productFound = await releaseArtifactService.DeleteProductIfExists(product);

            if (productFound)
            {
                return Ok();
            }
            else
            {
                return NotFoundResponseFactory.Create(
                    this.HttpContext,
                    "Resource not found",
                    "The specified product does not exist.");
            }
        }

        #endregion
    }
}