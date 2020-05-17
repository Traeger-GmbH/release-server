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
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Services;

namespace ReleaseServer.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ReleaseArtifactController : ControllerBase
    {
        private IReleaseArtifactService ReleaseArtifactService;
        private ILogger Logger;

        public ReleaseArtifactController(ILogger<ReleaseArtifactController> logger,
            IReleaseArtifactService releaseArtifactService)
        {
            Logger = logger;
            ReleaseArtifactService = releaseArtifactService;
        }
        
        /// <summary>
        /// Uploads a specific release artifact.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <param name="artifact"></param>
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
            var validationResult = await ReleaseArtifactService.ValidateUploadPayload(artifact);

            if (!validationResult.IsValid)
                return BadRequest(validationResult.ValidationError);
            
            await ReleaseArtifactService.StoreArtifact(product, os, architecture, version, artifact);
            
            return Ok("Upload of the artifact successful!");
        }
        
        /// <summary>
        /// Retrieves a list of all available versions of the specified product.
        /// </summary>
        /// <param name="product"></param>
        /// <response code="200">A product with the specified product name exists.</response>
        /// <response code="404">The specified product does not exist.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductInformationListResponseModel), 200)]
        [HttpGet("versions/{product}")]
        public async Task<ActionResult<ProductInformationListResponseModel>> GetProductInfos([Required] string product)
        {
            var productInfos = await ReleaseArtifactService.GetProductInfos(product);

            if (productInfos.IsNullOrEmpty()) 
                return NotFound("The specified product was not found!");
            
            return productInfos.ToProductInfoListResponse();
        }

        /// <summary>
        /// Retrieves all available platforms for a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="version"></param>
        /// <response code="200">There are existing Platforms for the specified product name.</response>
        /// <response code="404">The product does not exist or there exists no platform for the specified product.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlatformsResponseModel), 200)]
        [HttpGet("platforms/{product}/{version}")]
        public async Task<ActionResult<PlatformsResponseModel>> GetPlatforms([Required] string product, [Required]string version)
        {
            var platformsList = await ReleaseArtifactService.GetPlatforms(product, version);

            if (platformsList.IsNullOrEmpty()) 
                return NotFound("The specified product was not found or there exists no platform for the specified product!");
            
            
            return platformsList.ToPlatformsResponse();
        }
        
        /// <summary>
        /// Retrieves the Release information of a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">The specific product exists.</response>
        /// <response code="404">The product with the specified product name does not exist. Therefore the release notes do not exist.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ReleaseInformationResponseModel), 200)]
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public async Task<ActionResult<ReleaseInformationResponseModel>> GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var releaseInfo = await ReleaseArtifactService.GetReleaseInfo(product, os, architecture, version);

            if (releaseInfo == null)
                return NotFound("The Release information does not exist (the specified product was not found)!");

            return releaseInfo.ToReleaseInformationResponse();
        }
        
        /// <summary>
        /// Retrieves all available versions that are fitting to a specific product / platform (HW architecture + OS).
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <response code="200">There are existing versions for the specified platform and product.</response>
        /// <response code="404">There exists no version for the specified platform / product.</response>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductVersionListResponseModel), 200)]
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public async Task<ActionResult<ProductVersionListResponseModel>> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            var productVersions = await ReleaseArtifactService.GetVersions(product, os, architecture);
            
            if (productVersions.IsNullOrEmpty()) 
                return NotFound("No versions for the specified platform / product name found!");

            return productVersions.ToProductVersionListResponse();
        }
        
        /// <summary>
        /// Retrieves the artifact of the specified product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">There exists a product with the specified parameters.</response>
        /// <response code="404">There exists no product with the specified parameters.</response>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await ReleaseArtifactService.GetSpecificArtifact(product, os, architecture, version);

            if (response == null)
                return NotFound("The specified product was not found!");

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
        /// Retrieves the latest artifact of a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <response code="200">The specified product exists (the ZIP file with the artifact will be retrieved)</response>
        /// <response code="404">The product is not available vor the specified platform (OS + arch)</response>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/latest")]
        public async Task<IActionResult>  GetLatestArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await ReleaseArtifactService.GetLatestArtifact(product, os, architecture);
            
            if (response == null)
                return NotFound("The specified product was not found!");

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
        /// Retrieves the latest version of a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <response code="200">The specified product exists.</response>
        /// <response code="404">The product is not available for the specified platform (OS + HW architecture)
        /// or the product with the specified product name does not exist</response>
        [AllowAnonymous]
        [HttpGet("latest/{product}/{os}/{architecture}")]
        [ProducesResponseType(typeof(ProductVersionResponseModel), 200)]
        public async Task<ActionResult<ProductVersionResponseModel>> GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            var latestVersion = await ReleaseArtifactService.GetLatestVersion(product, os, architecture);
            
            if (latestVersion.IsNullOrEmpty())
                return NotFound("The specified product was not found!");

            return latestVersion.ToProductVersionResponse();
        }
        
        /// <summary>
        /// Deletes the specified product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">The specified product got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        /// <response code="404">There exists no product with the specified product name.</response>
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var artifactFound = await ReleaseArtifactService.DeleteSpecificArtifactIfExists(product, os, architecture, version);
            
            if (!artifactFound) 
                return NotFound("The product you want to delete does not exist!");

            return Ok("artifact successfully deleted");
        }
        
        /// <summary>
        /// Deletes all products of a specific product name.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <response code="200">All products of the specified product name got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        /// <response code="404">There exists no product with the specified product name.</response>
        [HttpDelete("{product}")]
        public async Task<IActionResult> DeleteProduct ([Required] string product)
        {
            var productFound = await ReleaseArtifactService.DeleteProductIfExists(product);
            
            if (!productFound)
                return NotFound("The products you want to delete do not exist!");

            return Ok("product successfully deleted");
        }
        
        /// <summary>
        /// Backups the whole artifact directory and retrieves it as a ZIP file.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">The artifact directory backup was successful.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        [HttpGet("backup")]
        public async Task<FileStreamResult> Backup()
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var backupInfo = await ReleaseArtifactService.RunBackup();
            
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
        /// <param name="backupFile"></param>
        /// <returns></returns>
        /// <response code="200">The restore process was successful.</response>
        /// <response code="400">No body provided.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
        [HttpPut("restore")]
        public async Task<IActionResult> Restore([Required] IFormFile backupFile)
        {
            if (backupFile == null)
                return BadRequest();
            
            await ReleaseArtifactService.RestoreBackup(backupFile);

            return Ok("backup successfully restored");
        }

        //Commented out, because swagger runs into a fetch error
       [AllowAnonymous]
       [ApiExplorerSettings(IgnoreApi = true)]
       [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return NotFound("");
        }
    }
}