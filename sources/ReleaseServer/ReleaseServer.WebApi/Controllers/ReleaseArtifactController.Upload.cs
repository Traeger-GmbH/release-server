//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Upload.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ReleaseServer.WebApi
{
    public partial class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Public methods ----------
        
        /// <summary>
        /// Uploads a specific release artifact.
        /// </summary>
        /// <param name="product">The product name of the uploaded artifact.</param>
        /// <param name="os">The operating system of the uploaded artifact.</param>
        /// <param name="architecture">The hardware architecture of the uploaded artifact.</param>
        /// <param name="version">The version of the uploaded artifact.</param>
        /// <param name="artifact">The payload of the uploaded artifact (Zip file).</param>
        /// <returns>Returns an <see cref="OkObjectResult"/>, if the upload was successful. Returns <see cref="BadRequestObjectResult"/>,
        /// if the payload is invalid.</returns>
        /// <response code="200">Upload of the artifact was successful.</response>
        /// <response code="400">No or invalid body provided (must be a Zip file).</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("upload/{product}/{os}/{architecture}/{version}")]
        //Max. 500 MB
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadSpecificArtifact([Required] string product, [Required] string os, 
            [Required] string architecture, [Required] string version, [Required] IFormFile artifact)
        {
            if (artifact == null)
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body is missing.");
            }
            else if (artifact.ContentType != "application/zip")
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body not a .zip file.");
            }
            else
            {
                //Validate the payload of the uploaded Zip file
                var validationResult = releaseArtifactService.ValidateUploadPayload(artifact);

                if (!validationResult.IsValid)
                {
                    return BadRequestResponseFactory.Create(
                        HttpContext,
                        "Bad request",
                        validationResult.ValidationError);
                }
            
                await releaseArtifactService.StoreArtifact(product, os, architecture, version, artifact);
                return Ok();
            }
        }
       
        #endregion
    }
}