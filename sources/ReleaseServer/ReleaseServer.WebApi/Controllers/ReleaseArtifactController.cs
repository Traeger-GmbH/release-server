//--------------------------------------------------------------------------------------------------
// <copyright file="ReleaseArtifactController.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Traeger</author>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// A secured ApiController that provides several endpoints for managing release artifacts (upload / download
    /// artifacts, get several information about the stored artifacts)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("artifacts")]
    public partial class ReleaseArtifactController : ControllerBase
    {
        #region ---------- Private fields ----------

        private IReleaseArtifactService releaseArtifactService;
        private ILogger logger;

        #endregion

        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseArtifactController"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> to enable logging for the <see cref="ReleaseArtifactController"/>.</param>
        /// <param name="releaseArtifactService">The service that handles the business logic of the application.</param>
        public ReleaseArtifactController(ILogger<ReleaseArtifactController> logger,
            IReleaseArtifactService releaseArtifactService)
        {
            this.logger = logger;
            this.releaseArtifactService = releaseArtifactService;
        }
        #endregion
        
        #region ---------- Public methods ----------
        
        /// <summary>
        /// Catches all other routes, which are not defined and returns a <see cref="NotFoundObjectResult"/>.
        /// </summary>
        /// <returns>An <see cref="NotFoundObjectResult"/>.</returns>
        [AllowAnonymous]
        [ApiExplorerSettings(IgnoreApi = true)]
        [Route("{*url}", Order = 999)]
        public IActionResult CatchAll()
        {
            return NotFound("");
        }

        #endregion

        #region ---------- Private methods ----------

        private static int CompareByVersion(DeploymentInformation x, DeploymentInformation y)
        {
            return x.Version.CompareTo(y.Version);
        }

        private static int CompareByVersion(ReleaseInformation x, ReleaseInformation y)
        {
            return x.Version.CompareTo(y.Version);
        }

        #endregion
    }
}
