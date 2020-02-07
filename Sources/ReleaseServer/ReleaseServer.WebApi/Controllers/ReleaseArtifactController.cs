using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using ReleaseServer.WebApi.Repositories;
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
        private JsonSerializerOptions JsonSerializerOptions;


        //TODO: Refactor this kind of logging! 
        public ReleaseArtifactController(ILogger<ReleaseArtifactController> logger,
            IReleaseArtifactService releaseArtifactService)
        {
            Logger = logger;
            ReleaseArtifactService = releaseArtifactService;
            JsonSerializerOptions = new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
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
        public string GetProductInfos([Required] string product)
        {
            return JsonSerializer.Serialize(ReleaseArtifactService.GetProductInfos(product),JsonSerializerOptions);
        }

        [AllowAnonymous]
        [HttpGet("platforms/{product}/{version}")]
        public string GetPlatforms([Required] string product, [Required]string version)
        {
            return JsonSerializer.Serialize(ReleaseArtifactService.GetPlatforms(product, version), JsonSerializerOptions);
        }
        
        [AllowAnonymous]
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public string GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            return ReleaseArtifactService.GetReleaseInfo(product, os, architecture, version);
        }
        
        [AllowAnonymous]
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public List<string> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            return ReleaseArtifactService.GetVersions(product, os, architecture);
        }
        
        [AllowAnonymous]
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
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

        [AllowAnonymous]
        [HttpGet("latest/{product}/{os}/{architecture}")]
        public string GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            return ReleaseArtifactService.GetLatestVersion(product, os, architecture);
        }
        
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public IActionResult DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            ReleaseArtifactService.DeleteSpecificArtifact(product, os, architecture, version);

            return Ok("artifact successfully deleted");
        }
        
        [HttpDelete("{product}")]
        public IActionResult DeleteProduct ([Required] string product)
        {
            ReleaseArtifactService.DeleteProduct(product);

            return Ok("product successfully deleted");
        }

        [AllowAnonymous]
        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return NotFound("");
        }
    }
}