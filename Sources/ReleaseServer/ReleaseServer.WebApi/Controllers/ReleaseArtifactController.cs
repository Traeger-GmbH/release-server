using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        
        [HttpPut("upload/{product}/{os}/{architecture}/{version}")]
        //Max. 500 MB
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var file = Request.Form.Files.FirstOrDefault();
            
            if (file == null)
                return BadRequest();
            
            await ReleaseArtifactService.StoreArtifact(product, os, architecture, version, file);

            return Ok("Upload of the artifact successful!");
        }
        
        [AllowAnonymous]
        [HttpGet("versions/{product}")]
        public async Task<ProductInformationListResponseModel> GetProductInfos([Required] string product)
        {
            var productInfos = await ReleaseArtifactService.GetProductInfos(product);
            
            return productInfos.ToProductInfoListResponse();
        }

        [AllowAnonymous]
        [HttpGet("platforms/{product}/{version}")]
        public async Task<PlatformsResponseModel> GetPlatforms([Required] string product, [Required]string version)
        {
            var platformsList = await ReleaseArtifactService.GetPlatforms(product, version);

            return platformsList.ToPlatformsResponse();
        }
        
        [AllowAnonymous]
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public async Task<ChangelogResponseModel> GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var releaseInfo = await ReleaseArtifactService.GetReleaseInfo(product, os, architecture, version);

            return releaseInfo.toChangelogResponse();
        }
        
        [AllowAnonymous]
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public async Task<ProductVersionListResponseModel> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            var productVersions = await ReleaseArtifactService.GetVersions(product, os, architecture);

            return productVersions.ToProductVersionListResponse();
        }
        
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await ReleaseArtifactService.GetSpecificArtifact(product, os, architecture, version);

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
        
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/latest")]
        public async Task<IActionResult>  GetLatestArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {
            var provider = new FileExtensionContentTypeProvider();
            string contentType;

            var response = await ReleaseArtifactService.GetLatestArtifact(product, os, architecture);

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

        [AllowAnonymous]
        [HttpGet("latest/{product}/{os}/{architecture}")]
        public async Task<ProductVersionResponseModel> GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            var latestVersion = await ReleaseArtifactService.GetLatestVersion(product, os, architecture);

            return latestVersion.ToProductVersionResponse();
        }
        
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            await ReleaseArtifactService.DeleteSpecificArtifact(product, os, architecture, version);

            return Ok("artifact successfully deleted");
        }
        
        [HttpDelete("{product}")]
        public async Task<IActionResult> DeleteProduct ([Required] string product)
        {
            await ReleaseArtifactService.DeleteProduct(product);

            return Ok("product successfully deleted");
        }
        
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

        [HttpPut("restore")]
        public async Task<IActionResult> Restore()
        {
            var payload = Request.Form.Files.FirstOrDefault();
            
            if (payload == null)
                return BadRequest();
            
            await ReleaseArtifactService.RestoreBackup(payload);

            return Ok("backup successfully restored");
        }

        [AllowAnonymous]
        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return NotFound("");
        }
    }
}