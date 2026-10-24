//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Maintenance.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// A secured ApiController that provides several endpoints for managing release artifacts (upload / download
    /// artifacts, get several information about the stored artifacts)
    /// </summary>
    public partial class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Public methods ----------
        
        /// <summary>
        /// Creates a backup of all products and returns it as ZIP file.
        /// </summary>
        /// <remarks>
        /// The backup is also kept on the server (the newest backups, see the setting BackupRetentionCount).
        /// It can be restored with PUT /artifacts/restore.
        /// </remarks>
        /// <returns>The created <see cref="FileStreamResult"/> with the backup.</returns>
        /// <response code="200">The backup (ZIP file).</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        [HttpGet("backup")]
        [EnableCors(CorsPolicies.Authenticated)]
        public async Task<FileStreamResult> Backup()
        {

            var backupInfo = await releaseArtifactService.RunBackup();
            
            var stream = new FileStream(backupInfo.FullPath, FileMode.Open, FileAccess.Read);

            // Determine the content type

            var contentType = ArtifactContentTypeProvider.GetContentType(backupInfo.FileName);
            
            return new FileStreamResult(stream, contentType)
            {
                FileDownloadName = backupInfo.FileName
            };
        }

        /// <summary>
        /// Replaces all products by the content of a backup.
        /// </summary>
        /// <remarks>
        /// The backup is extracted and verified first (structure, meta information, release notes and files).
        /// The existing data is only replaced if the backup is valid; otherwise it stays unchanged and the
        /// response describes the problems. Every release needs its deployment.json, release notes and
        /// artifact file; additional files and directories (e.g. stored by previous versions) are kept.
        /// </remarks>
        /// <param name="backupFile">The backup (ZIP file created by GET /artifacts/backup), sent as multipart/form-data.</param>
        /// <returns>An <see cref="OkObjectResult"/> if the restore operation was successful and <see cref="BadRequestResult"/>, if not.</returns>
        /// <response code="200">The backup was restored.</response>
        /// <response code="400">The file is missing or the backup is invalid (the existing data is unchanged).</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        [HttpPut("restore")]
        [EnableCors(CorsPolicies.Authenticated)]
        // Backups contain all artifacts and can be large; the endpoint requires authentication.
        [DisableRequestSizeLimit]
        [RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue)]
        public async Task<IActionResult> Restore([Required] IFormFile backupFile)
        {
            if (backupFile != null)
            {
                try
                {
                    await releaseArtifactService.RestoreBackup(backupFile);
                }
                catch (BackupValidationException e)
                {
                    return BadRequestResponseFactory.Create(
                        HttpContext,
                        "Bad request",
                        e.Message);
                }
                return Ok();
            }
            else
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required backup file is missing.");
            }            

        }

        /// <summary>
        /// Returns statistics of the server.
        /// </summary>
        /// <remarks>
        /// Disk usage in MB (total size and free space of the disk, space used by the artifacts) and the number
        /// of products and artifacts (releases per platform).
        /// </remarks>
        /// <response code="200">The statistics.</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        [HttpGet("statistics")]
        [EnableCors(CorsPolicies.Authenticated)]
        public async Task<ActionResult<Statistics>> GetStatistics()
        {
            return await this.releaseArtifactService.GetStatistics();
        }

        #endregion
    }
}
