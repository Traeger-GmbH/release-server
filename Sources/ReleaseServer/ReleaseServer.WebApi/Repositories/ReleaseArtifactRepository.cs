using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
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

        public FsReleaseArtifactRepository(ILogger<FsReleaseArtifactRepository> logger, string artifactRoot)
        {
            ArtifactRoot = artifactRoot;
            Logger = logger;
        }
        
        public async Task StoreArtifact(ReleaseArtifactModel artifact)
        {
            
           var path = GenerateArtifactPath(
                artifact.ProductInformation.ProductIdentifier,
                artifact.ProductInformation.Os,
                artifact.ProductInformation.HwArchitecture,
                artifact.ProductInformation.Version.ToString());

           var tmpPath = GenerateTemporaryPath();

            try
            {
                //Create the temporary directory
                if (!Directory.Exists(tmpPath))
                    Directory.CreateDirectory(tmpPath);
                
                //Extract the payload to the temporary directory
                await Task.Run(() => artifact.Payload.ExtractToDirectory(tmpPath));
                Logger.LogDebug("The Artifact was successfully unpacked & stored to the temp directory");
                
                //If the directory already exists, delete the old content in there
                if (Directory.Exists(path))
                {
                    Logger.LogInformation("This path already exits! Old content will be deleted!");
                    
                    var dirInfo = new DirectoryInfo(path);
                    dirInfo.Delete(true);
                    
                    Logger.LogInformation("Old path successfully deleted!");
                }
                else
                {
                    //Create the directory & delete the last directory hierarchy of the path
                    //(this is necessary, so that Directory.Move() below does not fail with "Directory already exists" 
                    var dirInfo = Directory.CreateDirectory(path);
                    dirInfo.Delete();
                    Logger.LogInformation("The directory {0} was successfully created", dirInfo.Parent.FullName);
                }
                
                Directory.CreateDirectory(Path.Combine(ArtifactRoot, artifact.ProductInformation.ProductIdentifier));
                
                //Move the extracted payload to the right directory
                Directory.Move(tmpPath, path);
                Logger.LogInformation("The Artifact was successfully stored");
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
                var path = GenerateArtifactPath(product, os, architecture, version);

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

        public ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version)
        {
            try
            {
                var path = GenerateArtifactPath(productName, os, architecture, version);
                
                if (Directory.Exists(path))
                {
                    var dir = new DirectoryInfo(path);
                    var files = dir.GetFiles();
                    
                    var deploymentMetaInfo = GetDeploymentMetaInfo(files);
                    
                    
                    return new ArtifactDownloadModel
                    {
                        Payload = File.ReadAllBytes(Path.Combine(path, deploymentMetaInfo.ArtifactFileName)),
                        FileName = deploymentMetaInfo.ArtifactFileName,
                    };
                    
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
            var path = GenerateArtifactPath(productName, os, architecture, version);
            
            Directory.Delete(path, true);
        }

        public void DeleteProduct(string productName)
        {
            var path = Path.Combine(ArtifactRoot, productName);
            
            Directory.Delete(path, true);
        }

        private string GenerateArtifactPath(string product, string os, string architecture, string version)
        {
            return Path.Combine(ArtifactRoot, product, os, architecture, version);
        }
        
        private string GenerateTemporaryPath()
        {
            return Path.Combine(ArtifactRoot, "temp", Guid.NewGuid().ToString());
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
        ArtifactDownloadModel GetSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteSpecificArtifact(string productName, string os, string architecture, string version);
        void DeleteProduct(string productName);
    }
}




