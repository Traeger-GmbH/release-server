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
        /// Backups the whole artifact directory and retrieves it as a ZIP file.
        /// </summary>
        /// <returns>The created <see cref="FileStreamResult"/> with the backup.</returns>
        /// <response code="200">The artifact directory backup was successful.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
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
        /// Restores the uploaded backup file.
        /// </summary>
        /// <param name="backupFile">The uploaded backup file (ZIP file).</param>
        /// <returns>An <see cref="OkObjectResult"/> if the restore operation was successful and <see cref="BadRequestResult"/>, if not.</returns>
        /// <response code="200">The restore process was successful.</response>
        /// <response code="400">No body provided.</response>
        /// <response code="401">The user is not authorized (wrong credentials or missing auth header).</response>
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
        /// Gets statistics of the server like disk usage, number of products and artifacts.
        /// </summary>
        /// <response code="200">The restore process was successful.</response>
        [HttpGet("statistics")]
        [EnableCors(CorsPolicies.Authenticated)]
        public async Task<ActionResult<Statistics>> GetStatistics()
        {
            return await this.releaseArtifactService.GetStatistics();
        }

        #endregion
    }
}
