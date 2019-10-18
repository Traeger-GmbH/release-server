using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        [HttpPut("upload/{product}/{version}/{os}/{architecture}")]
        public async Task<IActionResult> UploadArtifact(string product, string version, string os, string architecture)
        {
            var file = Request.Form.Files.FirstOrDefault();
            
            if (file == null)
                return BadRequest();
            
            await FsReleaseArtifactService.StoreArtifact(product, version, os, architecture, file);

            return Ok("Upload of the artifact successful!");
        }
        
        [HttpGet]
        public Task<string> GetArtifact()
        {
            return FsReleaseArtifactService.Get();
        }
    }
}