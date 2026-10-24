//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Download.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
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
        /// Downloads a specific release.
        /// </summary>
        /// <remarks>
        /// Returns the file of the release for the given platform. The file name is sent in the
        /// Content-Disposition header. Range requests are supported, so interrupted downloads can be resumed.
        ///
        /// Example:
        ///
        ///     GET /artifacts/example-product/windows/x64/1.2.0
        /// </remarks>
        /// <param name="product">The identifier of the product (case-sensitive).</param>
        /// <param name="os">The operating system, e.g. "windows", "linux" or "any".</param>
        /// <param name="architecture">The architecture, e.g. "x64", "x86", "arm64" or "any".</param>
        /// <param name="version">The version, e.g. "1.2.0" or "2.0.0-beta".</param>
        /// <returns>A <see cref="PhysicalFileResult"/> with the specified artifact. A <see cref="NotFoundObjectResult"/>, if
        /// the specified artifact does not exist.</returns>
        /// <response code="200">The file of the release.</response>
        /// <response code="206">A part of the file (range request).</response>
        /// <response code="404">The release does not exist for this platform.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [HttpGet("{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {

            var response = await releaseArtifactService.GetSpecificArtifact(product, os, architecture, version);

            if (response != null)
            {
                // Determine the content type
                var contentType = ArtifactContentTypeProvider.GetContentType(response.FileName);

                //Set the filename of the response
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = response.FileName
                };
                Response.Headers["Content-Disposition"] = cd.ToString();

                // Stream the file from disk; range requests allow resuming interrupted downloads
                return new PhysicalFileResult(response.FilePath, contentType) { EnableRangeProcessing = true };
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
        /// Downloads the latest release.
        /// </summary>
        /// <remarks>
        /// Returns the file of the newest release (highest version) for the given platform. Pre-releases are
        /// included: a pre-release is delivered if it has a higher version than the newest release. Use
        /// GET /artifacts/{product}/info to find out which version this is.
        ///
        /// Example:
        ///
        ///     GET /artifacts/example-product/windows/x64/latest
        /// </remarks>
        /// <param name="product">The identifier of the product (case-sensitive).</param>
        /// <param name="os">The operating system, e.g. "windows", "linux" or "any".</param>
        /// <param name="architecture">The architecture, e.g. "x64", "x86", "arm64" or "any".</param>
        /// <returns>A <see cref="PhysicalFileResult"/> with the latest version of the artifact. A <see cref="NotFoundObjectResult"/>, if
        /// the artifact is not available for the specified platform (OS + arch)</returns>
        /// <response code="200">The file of the latest release.</response>
        /// <response code="206">A part of the file (range request).</response>
        /// <response code="404">The product has no release for this platform.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(FileResult), 200, "application/octet-stream")]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [HttpGet("{product}/{os}/{architecture}/latest")]
        public async Task<IActionResult>  GetLatestArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {

            var response = await releaseArtifactService.GetLatestArtifact(product, os, architecture);

            if (response != null)
            {
                // Determine the content type
                var contentType = ArtifactContentTypeProvider.GetContentType(response.FileName);

                //Set the filename of the response
                var cd = new System.Net.Mime.ContentDisposition
                {
                    FileName = response.FileName
                };
                Response.Headers["Content-Disposition"] = cd.ToString();

                // Stream the file from disk; range requests allow resuming interrupted downloads
                return new PhysicalFileResult(response.FilePath, contentType) { EnableRangeProcessing = true };
            }
            else
            {
                return NotFoundResponseFactory.Create(
                    this.HttpContext,
                    "Resource not found",
                    "The specified artifact does not exist.");
            }
        }

        #endregion
    }
}