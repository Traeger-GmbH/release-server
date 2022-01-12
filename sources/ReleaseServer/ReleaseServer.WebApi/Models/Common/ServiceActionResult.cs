//--------------------------------------------------------------------------------------------------
// <copyright file="ServiceActionResult.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Net;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceActionResult
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// 
        /// </summary>
        public ServiceActionResult(HttpStatusCode status)
        {
            this.Status = status;
        }

        /// <summary>
        /// 
        /// </summary>
        public ServiceActionResult(HttpStatusCode status, IEnumerable<string> errors)
            : this(status)
        {
            this.Errors = errors;
        }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// The status of the result.
        /// </summary>
        public HttpStatusCode Status { get; }

        /// <summary>
        /// Gets the action errors.
        /// </summary>
        public IEnumerable<string> Errors { get; }

        #endregion
    }
}
