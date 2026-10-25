//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactHashBackfillService.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Adds the SHA-256 checksums of artifacts uploaded before version 1.1 in the background after the start
    /// (and after a restore). The server is fully available in the meantime; /info returns no checksum for
    /// such artifacts until it is calculated.
    /// </summary>
    internal class ArtifactHashBackfillService : BackgroundService
    {
        private readonly IReleaseArtifactService releaseArtifactService;
        private readonly ILogger<ArtifactHashBackfillService> logger;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);
        private const int MaximumRetries = 10;

        private readonly SemaphoreSlim trigger = new SemaphoreSlim(1);
        private int retries;

        public ArtifactHashBackfillService(IReleaseArtifactService releaseArtifactService, ILogger<ArtifactHashBackfillService> logger)
        {
            this.releaseArtifactService = releaseArtifactService;
            this.logger = logger;
        }

        /// <summary>
        /// Requests another run, e.g. after a restore.
        /// </summary>
        public void Trigger()
        {
            if (trigger.CurrentCount == 0)
            {
                trigger.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await trigger.WaitAsync(stoppingToken);

                try
                {
                    await releaseArtifactService.AddMissingArtifactHashes(stoppingToken);

                    // Checksums that could not be stored (e.g. a file in use) are tried again later, a limited
                    // number of times per start (a permanently unreadable file is not hashed again and again)
                    if (retries < MaximumRetries && releaseArtifactService.HasMissingArtifactHashes())
                    {
                        retries++;
                        _ = Task.Delay(RetryDelay, stoppingToken).ContinueWith(_ => Trigger(), TaskScheduler.Default);
                    }
                }
                catch (OperationCanceledException)
                {
                    return;
                }
                catch (Exception e)
                {
                    logger.LogError("Adding the missing checksums failed: {0}", e.Message);
                }
            }
        }
    }
}
