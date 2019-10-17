using System;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace release_server_web_api.Services
{
    public class FsReleaseArtifactService : IReleaseArtifactService
    {
        private readonly string ArtifactRoot;

        public FsReleaseArtifactService(string artifactRoot = "./")
        {
            ArtifactRoot = artifactRoot;
        }
        
        public async Task UploadArtifact(string version, string os, string architecture, IFormFile payload)
        {
            var path = Path.Combine(ArtifactRoot, version, os, architecture);

            try
            {
                //If the directory already exists, delete the old content in there
                if (Directory.Exists(path))
                {
                    Console.WriteLine("This path already exits! Old content will be deleted!");
                    
                    var dirInfo = new DirectoryInfo(path);
                        
                    foreach (var file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    
                    Console.Write("Old path successfully deleted!");
                }
                else
                {
                    //Create the directory
                    var dirInfo = Directory.CreateDirectory(path);
                    Console.WriteLine("The directory {0} was successfully created", dirInfo.FullName);
                }
                
                //Create the file
                using (var fs = File.Create(Path.Combine(path, payload.FileName)))
                {
                   await payload.CopyToAsync(fs);
                   Console.WriteLine("The file {0} was successfully created", payload.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<string> Get()
        {
            return "this is a test artifact";
        }
    }
    
    public interface IReleaseArtifactService
    {
        Task UploadArtifact(string version, string os, string architecture, IFormFile payload);
        Task<string> Get();
    }
}