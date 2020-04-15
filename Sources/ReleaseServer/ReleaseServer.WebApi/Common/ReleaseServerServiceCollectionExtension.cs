using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Repositories;
using ReleaseServer.WebApi.Services;

namespace ReleaseServer.WebApi.Common
{
    public static class ReleaseServerServiceCollectionExtension
    {
        public static IServiceCollection AddFsReleaseArtifactService(this IServiceCollection services, IConfiguration configuration)
        {
            //Check file permissions
            var artifactRootDir = configuration["ArtifactRootDirectory"];
            var backupRootDir = configuration["BackupRootDirectory"];
            
            CheckPermissions(artifactRootDir);
            CheckPermissions(backupRootDir);
            
            services.AddSingleton<IReleaseArtifactService>(serviceProvider =>
            {
                var releaseArtifactRepository = new FsReleaseArtifactRepository(
                    serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactRepository>>(),
                    serviceProvider.GetRequiredService<IConfiguration>()
                );
                return new FsReleaseArtifactService(
                    releaseArtifactRepository,
                    serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactService>>()
                );
            });
            return services;
        }
        
        private static void CheckPermissions(string directoryToTest)
        {
            if (directoryToTest == null)
                return;
            
            var directoryInfo = new DirectoryInfo(directoryToTest);

            var canWrite = directoryInfo.CanWriteDirectory();
            
            if (!canWrite)
            {
                throw new UnauthorizedAccessException($"Unable to write to the directory {directoryToTest}. Please check your permissions!");
            }
        }
    }
}