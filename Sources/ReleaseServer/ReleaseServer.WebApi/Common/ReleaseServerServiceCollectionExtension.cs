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
            var artifactRootDir = new DirectoryInfo(configuration["ArtifactRootDirectory"]);
            var backupRootDir = new DirectoryInfo(configuration["BackupRootDirectory"]);

            if (!artifactRootDir.Exists) {
                artifactRootDir.Create();
            }
            if (!backupRootDir.Exists) {
                backupRootDir.Create();
            }

            artifactRootDir.EnsureWritable();
            backupRootDir.EnsureWritable();
            
            services.AddSingleton<IReleaseArtifactService>(serviceProvider =>
            {
                var releaseArtifactRepository = new FsReleaseArtifactRepository(
                    serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactRepository>>(),
                    artifactRootDir,
                    backupRootDir
                );
                return new FsReleaseArtifactService(
                    releaseArtifactRepository,
                    serviceProvider.GetRequiredService<ILogger<FsReleaseArtifactService>>()
                );
            });
            return services;
        }
    }
}