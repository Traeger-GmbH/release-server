//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Delete.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

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
        /// Deletes a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <param name="version">The version of the specified artifact.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the deletion was successful and <see cref="NotFoundObjectResult"/>, if not.</returns>
        /// <response code="200">The specified artifact got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        /// <response code="404">There exists no artifact with the specified parameters.</response>
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var artifactFound = await releaseArtifactService.DeleteSpecificArtifactIfExists(product, os, architecture, version);
            
            if (!artifactFound) 
                return NotFound("The artifact you want to delete does not exist!");

            return Ok("artifact successfully deleted");
        }
        
        /// <summary>
        /// Deletes all artifacts of a specific product name.
        /// </summary>
        /// <param name="product">The product name of the artifacts, that have to be deleted.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the deletion was successful and <see cref="NotFoundObjectResult"/>, if not.</returns>
        /// <response code="200">All artifacts of the specified product name got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        /// <response code="404">There exists no artifact with the specified product name.</response>
        [HttpDelete("{product}")]
        public async Task<IActionResult> DeleteProduct ([Required] string product)
        {
            var productFound = await releaseArtifactService.DeleteProductIfExists(product);
            
            if (!productFound)
                return NotFound("The artifacts you want to delete do not exist!");

            return Ok("artifacts successfully deleted");
        }

        #endregion
    }
}