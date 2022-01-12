//--------------------------------------------------------------------------------------------------
// <copyright file="ServiceActionResult{T}.cs" company="Traeger Industry Components GmbH">
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
    public class ServiceActionResult<TResult> : ServiceActionResult
    {
        #region ---------- Public constructors ----------

        /// <summary>
        /// 
        /// </summary>
        public ServiceActionResult(TResult result)
            : base(HttpStatusCode.OK)
        {
            this.Result = result;
        }

        /// <summary>
        /// 
        /// </summary>
        public ServiceActionResult(HttpStatusCode status, IEnumerable<string> errors)
            : base(status, errors)
        {
        }

        #endregion

        #region ---------- Public properties ----------

        /// <summary>
        /// Gets the result of the service action.
        /// </summary>
        public TResult Result { get; }

        #endregion
    }
}
