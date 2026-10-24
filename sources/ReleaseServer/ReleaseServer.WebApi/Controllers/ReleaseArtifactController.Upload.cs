//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.Upload.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace ReleaseServer.WebApi
{
    public partial class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Public methods ----------
        
        /// <summary>
        /// Uploads a release for one platform.
        /// </summary>
        /// <remarks>
        /// The request body is multipart/form-data with the field "artifact": a ZIP file (max. 500 MB) containing
        ///
        /// - deployment.json: { "artifactFileName": "setup.exe", "releaseNotesFileName": "releaseNotes.json" }
        /// - the release notes file: { "releaseDate": "2026-01-31", "isPreviewRelease": false, "isSecurityPatch": false,
        ///   "changes": { "en": [ { "added": [], "fixed": [], "breaking": [], "deprecated": [] } ] } }
        /// - the artifact file itself
        ///
        /// Operating system metadata in the ZIP file is ignored and not stored: macOS ("__MACOSX/", "._*",
        /// ".DS_Store") and Windows ("Thumbs.db", "ehthumbs.db", "desktop.ini", "$RECYCLE.BIN/").
        ///
        /// An existing release with the same product, platform and version is replaced. Names may contain
        /// letters, digits, '-', '_' and '.'.
        /// </remarks>
        /// <param name="product">The identifier of the product.</param>
        /// <param name="os">The operating system of the release, e.g. "windows", "linux" or "any".</param>
        /// <param name="architecture">The architecture of the release, e.g. "x64", "arm64" or "any".</param>
        /// <param name="version">The version of the release, e.g. "1.2.0" or "2.0.0-beta".</param>
        /// <param name="artifact">The ZIP file with deployment.json, release notes and artifact.</param>
        /// <returns>Returns an <see cref="OkObjectResult"/>, if the upload was successful. Returns <see cref="BadRequestObjectResult"/>,
        /// if the payload is invalid.</returns>
        /// <response code="200">The release was stored.</response>
        /// <response code="400">The file is missing or invalid, or a name is not allowed.</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("{product}/{os}/{architecture}/{version}")]
        [EnableCors(CorsPolicies.Authenticated)]
        //Max. 500 MB
        [RequestSizeLimit(524288000)]
        [RequestFormLimits(MultipartBodyLengthLimit = 524288000)]
        public async Task<IActionResult> UploadSpecificArtifact([Required] string product, [Required] string os, 
            [Required] string architecture, [Required] string version, [Required] IFormFile artifact)
        {
            if (artifact == null)
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body is missing.");
            }
            else if (artifact.ContentType != "application/zip")
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body not a .zip file.");
            }
            else if (!StorageNames.IsValidSegment(product) || !StorageNames.IsValidSegment(os)
                     || !StorageNames.IsValidSegment(architecture) || !StorageNames.IsValidVersion(version))
            {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "Invalid product, operating system, architecture or version. Allowed are letters, digits, '-', '_' and '.' (starting with a letter or digit); the version has to be a valid version number like 1.2.3 or 1.2.3-beta.");
            }
            else
            {
                //Validate the payload of the uploaded Zip file
                var validationResult = releaseArtifactService.ValidateUploadPayload(artifact);

                if (!validationResult.IsValid)
                {
                    var builder = new StringBuilder();
                    foreach (var error in validationResult.ValidationErrors) {
                        builder.Append(error);
                        builder.Append(' ');
                    }
                    return BadRequestResponseFactory.Create(
                        HttpContext,
                        "Bad request",
                        builder.ToString());
                }
            
                await releaseArtifactService.StoreArtifact(product, os, architecture, version, artifact);
                return Ok();
            }
        }

        /// <summary>
        /// Uploads a release for several platforms (package).
        /// </summary>
        /// <remarks>
        /// The request body is multipart/form-data with the field "package": a ZIP file (max. 1 GB) containing a
        /// folder with package.json and the files of all platforms:
        ///
        ///     {
        ///       "identifier": "example-product",
        ///       "version": "1.2.0",
        ///       "releaseNotes": { "releaseDate": "2026-01-31", "changes": { "en": [ { "added": ["..."] } ] } },
        ///       "deployments": {
        ///         "windows/x64": "files/setup-x64.exe",
        ///         "linux/arm64": "files/app-arm64.tar.gz",
        ///         "any/any": "files/documentation.zip"
        ///       }
        ///     }
        ///
        /// Operating system metadata in the ZIP file is ignored and not stored: macOS ("__MACOSX/", "._*",
        /// ".DS_Store") and Windows ("Thumbs.db", "ehthumbs.db", "desktop.ini", "$RECYCLE.BIN/").
        ///
        /// If the version already exists, the upload is rejected unless force=true is set.
        /// </remarks>
        /// <param name="package">The ZIP file with package.json and the files of all platforms.</param>
        /// <param name="force">Replace the release if this version of the product already exists.</param>
        /// <returns>Returns an <see cref="OkObjectResult"/>, if the upload was successful. Returns <see cref="BadRequestObjectResult"/>,
        /// if the payload is invalid.</returns>
        /// <response code="200">The release was stored for all platforms.</response>
        /// <response code="400">The file is missing or invalid, a name is not allowed, or the version already exists (title "Conflict").</response>
        /// <response code="401">Authentication is missing or the credentials are wrong.</response>
        /// <response code="500">Internal error.</response>
        [HttpPut("")]
        [EnableCors(CorsPolicies.Authenticated)]
        //Max. 1024 MB
        [RequestSizeLimit(1073741824)]
        [RequestFormLimits(MultipartBodyLengthLimit = 1073741824)]
        public async Task<IActionResult> UploadPackage([Required] IFormFile package, [FromQuery] bool force = false)
        {
            if (package == null) {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body is missing.");
            }
            else if (package.ContentType != "application/zip" && package.ContentType != "application/x-zip-compressed") {
                return BadRequestResponseFactory.Create(
                    HttpContext,
                    "Bad request",
                    "The required upload body is not a .zip file.");
            }
            else {
                var result = await this.releaseArtifactService.StorePackage(package, force);

                if (result.Status == HttpStatusCode.OK) {
                    return Ok();
                }
                else {
                    string title;
                    if (result.Status == HttpStatusCode.Conflict) {
                        title = "Conflict";
                    } else {
                        title = "Bad request";
                    }

                    var builder = new StringBuilder();
                    foreach (var error in result.Errors) {
                        builder.Append(error);
                        builder.Append(' ');
                    }

                    return BadRequestResponseFactory.Create(
                        HttpContext,
                        title,
                        builder.ToString());
                }
            }
        }

        #endregion
    }
}