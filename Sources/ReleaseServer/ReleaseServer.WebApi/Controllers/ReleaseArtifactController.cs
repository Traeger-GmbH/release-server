using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using release_server_web_api.Services;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Repositories;


namespace release_server_web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReleaseArtifactController : ControllerBase
    {
        private IReleaseArtifactService FsReleaseArtifactService;
        private ILogger Logger;

        //TODO: Refactor this kind of logging! 
        public ReleaseArtifactController(ILogger<ReleaseArtifactController> logger,
            ILogger<FsReleaseArtifactRepository> repositoryLogger,
            ILogger<FsReleaseArtifactService> serviceLogger
            )
        {
            FsReleaseArtifactService = new FsReleaseArtifactService(new FsReleaseArtifactRepository(repositoryLogger), serviceLogger);
            Logger = logger;
        }

        [HttpPut("upload/{product}/{os}/{architecture}/{version}")]
        //Max. 500 MB
        [RequestSizeLimit(524288000)]
        public async Task<IActionResult> UploadSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            var file = Request.Form.Files.FirstOrDefault();
            
            if (file == null)
                return BadRequest();
            
            await FsReleaseArtifactService.StoreArtifact(product, os, architecture, version, file);

            return Ok("Upload of the artifact successful!");
        }
        
        [HttpGet("versions/{product}")]
        public string GetProductInfos([Required] string product)
        {
            return JsonConvert.SerializeObject(FsReleaseArtifactService.GetProductInfos(product));
        }

        [HttpGet("platforms/{product}/{version}")]
        public string GetPlatforms([Required] string product, [Required]string version)
        {
            return JsonConvert.SerializeObject(FsReleaseArtifactService.GetPlatforms(product, version));
        }
        
        [HttpGet("info/{product}/{os}/{architecture}/{version}")]
        public string GetReleaseInfo([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            return FsReleaseArtifactService.GetReleaseInfo(product, os, architecture, version);
        }
        
        [HttpGet("versions/{product}/{os}/{architecture}")]
        public List<string> GetVersions([Required] string product, [Required] string os, [Required] string architecture)
        {
            return FsReleaseArtifactService.GetVersions(product, os, architecture);
        }
        
        [HttpGet("download/{product}/{os}/{architecture}/{version}")]
        public IActionResult  GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture, string version)
        {
            var result = new FileContentResult(FsReleaseArtifactService.GetSpecificArtifact(product, os, architecture, version),"application/octet-stream");
            return result;
        }
        
        [HttpGet("download/{product}/{os}/{architecture}/latest")]
        public IActionResult  GetSpecificArtifact([Required] string product, [Required] string os, [Required] string architecture)
        {
            var result = new FileContentResult(FsReleaseArtifactService.GetLatestArtifact(product, os, architecture),"application/octet-stream");
            return result;
        }

        [HttpGet("latest/{product}/{os}/{architecture}")]
        public string GetLatestVersion([Required] string product, [Required] string os, [Required] string architecture)
        {
            return FsReleaseArtifactService.GetLatestVersion(product, os, architecture);
        }
        
        [HttpDelete("{product}/{os}/{architecture}/{version}")]
        public IActionResult DeleteSpecificArtifact ([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
        {
            FsReleaseArtifactService.DeleteSpecificArtifact(product, os, architecture, version);

            return Ok("artifact successfully deleted");
        }
        
        [HttpDelete("{product}")]
        public IActionResult DeleteProduct ([Required] string product)
        {
            FsReleaseArtifactService.DeleteProduct(product);

            return Ok("product successfully deleted");
        }

    }
}