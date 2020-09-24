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
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Private fields ----------

        private IReleaseArtifactService releaseArtifactService;
        private ILogger logger;

        #endregion

        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseArtifactController"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to enable logging for the <see cref="ReleaseArtifactController"/>.</param>
        /// <param name="releaseArtifactService">The service that handles the business logic of the application.</param>
        public ReleaseArtifactController(ILogger<ReleaseArtifactController> logger,
            IReleaseArtifactService releaseArtifactService)
        {
            this.logger = logger;
            this.releaseArtifactService = releaseArtifactService;
        }
        #endregion
        
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
            
            if (artifact == null || artifact.ContentType != "application/zip")
                return BadRequest("No or invalid body provided (must be a Zip file)");
            
            //Validate the payload of the uploaded Zip file
            var validationResult = releaseArtifactService.ValidateUploadPayload(artifact);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.ValidationError);
            
            await releaseArtifactService.StoreArtifact(product, os, architecture, version, artifact);
            
            return Ok("Upload of the artifact successful!");
        }
        
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
        
        /// <summary>
        /// Backups the whole artifact directory and retrieves it as a ZIP file.
        /// </summary>
        /// <returns>The created <see cref="FileStreamResult"/> with the backup.</returns>
        /// <response code="200">The artifact directory backup was successful.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        [HttpGet("backup")]
        public async Task<FileStreamResult> Backup()
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var backupInfo = await releaseArtifactService.RunBackup();
            
            var stream = new FileStream(backupInfo.FullPath, FileMode.Open, FileAccess.Read);

            //Determine the content type
            if (!provider.TryGetContentType(backupInfo.FileName, out contentType))
            {
                contentType = "application/octet-stream";
            }
            
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = backupInfo.FileName
            };
        }

        /// <summary>
        /// Restores the uploaded backup file.
        /// </summary>
        /// <param name="backupFile">The uploaded backup file (ZIP file).</param>
        /// <returns>An <see cref="OkObjectResult"/> if the restore operation was successful and <see cref="BadRequestResult"/>, if not.</returns>
        /// <response code="200">The restore process was successful.</response>
        /// <response code="400">No body provided.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        [HttpPut("restore")]
        public async Task<IActionResult> Restore([Required] IFormFile backupFile)
        {
            if (backupFile == null)
                return BadRequest();
            
            await releaseArtifactService.RestoreBackup(backupFile);

            return Ok("backup successfully restored");
        }

       /// <summary>
       /// Catches all other routes, which are not defined and returns a <see cref="NotFoundObjectResult"/>.
       /// </summary>
       /// <returns>An <see cref="NotFoundObjectResult"/>.</returns>
       [AllowAnonymous]
       [ApiExplorerSettings(IgnoreApi = true)]
       [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return NotFound("");
        }
       
       #endregion
    }
}