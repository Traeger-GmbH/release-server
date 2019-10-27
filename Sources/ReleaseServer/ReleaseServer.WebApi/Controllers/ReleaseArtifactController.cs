using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using release_server_web_api.Services;


namespace release_server_web_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReleaseArtifactController : ControllerBase
    {
        private IReleaseArtifactService FsReleaseArtifactService { get; }

        public ReleaseArtifactController()
        {
            FsReleaseArtifactService = new FsReleaseArtifactService();
        }

        [HttpPut("upload/{product}/{os}/{architecture}/{version}")]
        public async Task<IActionResult> UploadArtifact([Required] string product, [Required] string os, [Required] string architecture, [Required] string version)
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
        
        [HttpGet]
        public Task<string> GetArtifact()
        {
            return FsReleaseArtifactService.Get();
        }
    }
}