using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Repositories
{
    public class FsReleaseArtifactRepository : IReleaseArtifactRepository
    {
        private readonly string ArtifactRoot;
        private ILogger Logger;

        public FsReleaseArtifactRepository(ILogger<FsReleaseArtifactRepository> logger, string artifactRoot = ".")
        {
            ArtifactRoot = artifactRoot;
            Logger = logger;
        }
        
        public async Task StoreArtifact(ReleaseArtifactModel artifact)
        {
            
           var path = GeneratePath(
                artifact.ProductInformation.ProductIdentifier,
                artifact.ProductInformation.Os,
                artifact.ProductInformation.HwArchitecture,
                artifact.ProductInformation.Version.ToString());

            try
            {
                //If the directory already exists, delete the old content in there
                if (Directory.Exists(path))
                {
                    Logger.LogInformation("This path already exits! Old content will be deleted!");
                    
                    var dirInfo = new DirectoryInfo(path);
                        
                    foreach (var file in dirInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    
                    Logger.LogInformation("Old path successfully deleted!");
                }
                else
                {
                    //Create the directory
                    var dirInfo = Directory.CreateDirectory(path);
                    Logger.LogInformation("The directory {0} was successfully created", dirInfo.FullName);
                }
                
                await Task.Run(() => artifact.Payload.ExtractToDirectory(path));
                Logger.LogInformation("The Artifact was successfully unpacked & stored");
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
                throw;
            }
        }

        public List<ProductInformationModel> GetInfosByProductName(string productName)
        {
            //Get all directories / subdirectories
            //TODO: filtering with File operations
            var directories = Directory.GetDirectories(Path.Combine(ArtifactRoot, productName), "*", SearchOption.AllDirectories);
            var retVal = new List<ProductInformationModel>();
            
            foreach (var directory in directories)
            {
                var productInfo = ProductInformationMapper.PathToProductInfo(ArtifactRoot, directory);
                    
                //Is null, if the directory hasn't a depth of 5
                if (productInfo != null)
                {
                    retVal.Add(productInfo);
                }
            }
            return retVal;
        }

        public string GetReleaseInfo(string product, string os, string architecture, string version)
        {
            try
            {
                var path = GeneratePath(product, os, architecture, version);

                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();
                    
                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);

                    var changelog = File.ReadAllText(Path.Combine(path, deploymentMetaInfo.ChangelogFileName));
                    return changelog;
                }
                
                throw new FileNotFoundException("Error: Release notes for this artifact not found!");
                
            }
            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
                throw;
            }
        }

        public byte[] GetSpecificArtifact(string productName, string os, string architecture, string version)
        {
            try
            {
                var path = GeneratePath(productName, os, architecture, version);
                
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();
                    
                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);
                    
                    byte[] artifact = File.ReadAllBytes(Path.Combine(path, deploymentMetaInfo.ArtifactFileName));
                    
                    return artifact;
                }
                Logger.LogInformation("The directory {0} does not exist!", path);
                throw new FileNotFoundException();
            }
            
            catch (Exception e)
            {
                Logger.LogCritical(e.Message);
                throw;
            }
        }

        public void DeleteSpecificArtifact(string productName, string os, string architecture, string version)
        {
            var path = GeneratePath(productName, os, architecture, version);
            
            Directory.Delete(path, true);
        }

        public void DeleteProduct(string productName)
        {
            var path = Path.Combine(ArtifactRoot, productName);
            
            Directory.Delete(path, true);
        }

        private string GeneratePath(string product, string os, string architecture, string version)
        {
            return Path.Combine(ArtifactRoot, product, os, architecture, version);
        }

        private DeploymentMetaInfo GetDeploymentMetaInfo(IEnumerable<FileInfo> fileInfos)
        {
            var deploymentMetaName = fileInfos.FirstOrDefault(f => f.Name == "deployment.json");
                    
            if (deploymentMetaName == null)
                throw new Exception("meta information of the specified product does not exist!");
                    
            return DeploymentMetaInfoMapper.ParseDeploymentMetaInfo(deploymentMetaName.FullName);
        } 
        
    }
    
    public interface IReleaseArtifactRepository     
    {
        Task StoreArtifact(ReleaseArtifactModel artifact);
        List<ProductInformationModel> GetInfosByProductName(string productName);
        string GetReleaseInfo(string product, string os, string architecture, string version);
        byte[] GetSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteProduct(string productName);
    }
}




