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

        [HttpGet]
        public Task<string> GetArtifact()
        {
            return FsReleaseArtifactService.Get();
        }

    }
}