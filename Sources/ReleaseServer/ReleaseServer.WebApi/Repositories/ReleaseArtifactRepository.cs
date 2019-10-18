using System;
using System.IO;
using System.Threading.Tasks;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Repositories
{
    public class FsReleaseArtifactRepository : IReleaseArtifactRepository
    {
        private readonly string ArtifactRoot;

        public FsReleaseArtifactRepository(string artifactRoot = "./")
        {
            ArtifactRoot = artifactRoot;
        }
        
        public async Task StoreArtifact(ReleaseArtifactModel artifact)
        {
            var path = Path.Combine(ArtifactRoot,
                artifact.ProductInformation.Version,
                artifact.ProductInformation.Os,
                artifact.ProductInformation.HwArchitecture);

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
                using (var fs = File.Create(Path.Combine(path, artifact.Payload.FileName)))
                {
                    await artifact.Payload.CopyToAsync(fs);
                    Console.WriteLine("The file {0} was successfully created", artifact.Payload.FileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
    
    public interface IReleaseArtifactRepository
    {
        Task StoreArtifact(ReleaseArtifactModel artifact);
    }
}




