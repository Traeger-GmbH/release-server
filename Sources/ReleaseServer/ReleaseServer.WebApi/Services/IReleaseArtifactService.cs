using System.Threading.Tasks;

namespace release_server_web_api.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        public async Task<string> Get()
        {
            return "this is a test artifact";
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task<string> Get();
    }
}