using System.ComponentModel.DataAnnotations;
using System.IO;
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
        /// <response code="400">No or wrong body provided.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("upload/{product}/{os}/{architecture}/{version}")]
        //Max. 500 MB
        [RequestSizeLimit(524288000)]
        public IActionResult UploadSpecificArtifact([Required] string product, [Required] string os, 
            [Required] string architecture, [Required] string version, [Required] IFormFile artifact)
        {
            
            if (artifact == null)
                return BadRequest();
            
            ReleaseArtifactService.StoreArtifact(product, os, architecture, version, artifact);

            return Ok("Upload of the artifact successful!");
        }
        
        /// <summary>
        /// Retrieves all available products.
        /// </summary>
        /// <param name="product"></param>
        /// <response code="200">A product with the specified product name exists.
        /// Empty productInformation, if there exists no product with the specified product name</response>
        /// <response code="500">Wrong body provided (not 'content-type: multipart/form-data').</response>
        /// <returns></returns>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductInformationListResponseModel), 200)]
        [HttpGet("versions/{product}")]
        public ProductInformationListResponseModel GetProductInfos([Required] string product)
        {
            return ReleaseArtifactService.GetProductInfos(product).ToProductInfoListResponse();
        }

        /// <summary>
        /// Retrieves all available platforms for a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="version"></param>
        /// <response code="200">There are existing Platforms for the specified product name. Empty platforms array, if
        /// there exists no platforms for the specified product name</response>
        /// <returns></returns>
        [AllowAnonymous]
        [ProducesResponseType(typeof(PlatformsResponseModel), 200)]
        [HttpGet("platforms/{product}/{version}")]
        public PlatformsResponseModel GetPlatforms([Required] string product, [Required]string version)
        {
            var platformsList = ReleaseArtifactService.GetPlatforms(product, version);

            return platformsList.ToPlatformsResponse();
        }
        
        /// <summary>
        /// Retrieves the changelog of a specific product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">The specific product exists.</response>
        /// <response code="500">The product with the specified product name does not exist
        /// or there is no changelog available.</response>
        /// <returns></returns>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ChangelogResponseModel), 200)]
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public ChangelogResponseModel GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            return ReleaseArtifactService.GetReleaseInfo(product, os, architecture, version).toChangelogResponse();
        }
        
        /// <summary>
        /// Retrieves all available versions that are fitting to a specific product / platform (HW architecture + OS).
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <response code="200">There are existing versions for the specified platform and product.
        /// Empty versions array, if there exists no version for the specified platform / product.</response>
        /// <response code="500">There exists no product with the specified product name.</response>
        /// <returns></returns>
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductVersionListResponseModel), 200)]
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public ProductVersionListResponseModel GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            return ReleaseArtifactService.GetVersions(product, os, architecture).ToProductVersionListResponse();
        }
        
        
        /// <summary>
        /// Retrieves the artifact of the specified product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">There exists a product with the specified product name / arch / version / os.</response>
        /// <response code="500">There exists no product with the specified product name / arch / version / os.</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
        [ProducesResponseType(typeof(byte), 200)]
        public IActionResult  GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = ReleaseArtifactService.GetSpecificArtifact(product, os, architecture, version);

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
        /// <response code="200">The specified product exists. The ZIP file with the artifact will be retrieved</response>
        /// <response code="500">The product is not available vor the specified platform (OS + arch)</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/latest")]
        public IActionResult  GetLatestArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = ReleaseArtifactService.GetLatestArtifact(product, os, architecture);

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
        /// <response code="500">The product is not available for the specified platform (OS + HW architecture)
        /// or the product with the specified product name does not exist</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("latest/{product}/{os}/{architecture}")]
        [ProducesResponseType(typeof(ProductVersionResponseModel), 200)]
        public ProductVersionResponseModel GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            return ReleaseArtifactService.GetLatestVersion(product, os, architecture).ToProductVersionResponse();
        }
        
        /// <summary>
        /// Deletes the specified product.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="os"></param>
        /// <param name="architecture"></param>
        /// <param name="version"></param>
        /// <response code="200">The specified product got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header.</response>
        /// <response code="500">There exists no product with the specified product name.</response>
        /// <returns></returns>
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public IActionResult DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            ReleaseArtifactService.DeleteSpecificArtifact(product, os, architecture, version);

            return Ok("artifact successfully deleted");
        }
        
        /// <summary>
        /// Deletes all products of a specific product name.
        /// </summary>
        /// <param name="product"></param>
        /// <returns></returns>
        /// <response code="200">All products of the specified product name got deleted successfully.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header.</response>
        /// <response code="500">There exists no product with the specified product name.</response>
        [HttpDelete("{product}")]
        [ProducesResponseType(typeof(IActionResult), 200)]
        public IActionResult DeleteProduct ([Required] string product)
        {
            ReleaseArtifactService.DeleteProduct(product);

            return Ok("product successfully deleted");
        }
        
        /// <summary>
        /// Backups the whole artifact directory and retrieves it as a ZIP file.
        /// </summary>
        /// <returns></returns>
        /// <response code="200">The artifact directory backup was successful.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header.</response>
        [HttpGet("backup")]
        [ProducesResponseType(typeof(IActionResult), 200)]
        [ProducesResponseType(typeof(void), 401)]
        public FileStreamResult Backup()
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var backupInfo = ReleaseArtifactService.RunBackup();
            
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
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header.</response>
        [HttpPut("restore")]
        public IActionResult Restore([Required] IFormFile backupFile)
        {
            if (backupFile == null)
                return BadRequest();
            
            ReleaseArtifactService.RestoreBackup(backupFile);

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