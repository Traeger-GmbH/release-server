//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseServerServiceCollectionExtension.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ReleaseServer.WebApi
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