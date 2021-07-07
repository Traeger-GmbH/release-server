//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

using Castle.Core.Internal;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

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
        /// Retrieves a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <param name="version">The version of the specified artifact.</param>
        /// <returns>A <see cref="FileContentResult"/> with the specified artifact. A <see cref="NotFoundObjectResult"/>, if
        /// the specified artifact does not exist.</returns>
        /// <response code="200">There exists an artifact with the specified parameters.</response>
        /// <response code="404">There exists no artifact with the specified parameters.</response>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await releaseArtifactService.GetSpecificArtifact(product, os, architecture, version);

            if (response == null)
                return NotFound("The specified artifact was not found!");

            //Determine the content type
            if (!provider.TryGetContentType(response.FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            
            //Set the filename of the response
            var cd = new ContentDispositionHeaderValue("attachment")
            {
                FileNameStar = response.FileName
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            
            return new FileContentResult(response.Payload, contentType);
        }
        
        /// <summary>
        /// Retrieves the latest version of a specific artifact.
        /// </summary>
        /// <param name="product">The product name of the specified artifact.</param>
        /// <param name="os">The operating system of the specified artifact.</param>
        /// <param name="architecture">The hardware architecture of the specified artifact.</param>
        /// <returns>A <see cref="FileContentResult"/> with the latest version of the artifact. A <see cref="NotFoundObjectResult"/>, if
        /// the artifact is not available for the specified platform (OS + arch)</returns>
        /// <response code="200">The specified artifact exists (the ZIP file with the artifact will be retrieved)</response>
        /// <response code="404">The artifact is not available for the specified platform (OS + arch)</response>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/latest")]
        public async Task<IActionResult>  GetLatestArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await releaseArtifactService.GetLatestArtifact(product, os, architecture);
            
            if (response == null)
                return NotFound("The specified artifact was not found!");

            //Determine the content type
            if (!provider.TryGetContentType(response.FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            
            //Set the filename of the response
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = response.FileName
            };
            Response.Headers.Add("Content-Disposition", cd.ToString());
            
            return new FileContentResult(response.Payload, contentType);
        }

        #endregion
    }
}