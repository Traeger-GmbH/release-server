//--------------------------------------------------------------------------------------------------
// <copyright file="NotFoundResponseFactory.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2013-2021.
// </copyright>
// <author>Fabian Traeger</author>
//--------------------------------------------------------------------------------------------------

namespace ReleaseServer.WebApi
{
    using System.Collections.Generic;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    internal static class NotFoundResponseFactory
    {
        #region ---------- Public static methods ----------

        public static NotFoundObjectResult Create(HttpContext context, string title, string detail, IDictionary<string, object> extensions = null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = title,
                Detail = detail,
            };

            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    problemDetails.Extensions.Add(extension);
                }
            }

            problemDetails.Extensions.Add("traceId", context.TraceIdentifier);

            return new NotFoundObjectResult(problemDetails);
        }

        public static NotFoundObjectResult Create(HttpContext context, string title, string detail, params (string name, object value)[] parameters)
        {
            var extensions = new Dictionary<string, object>();
            var parameterList = new Dictionary<string, object>();
            foreach (var (name, value) in parameters)
            {
                parameterList[name] = value;
            }
            extensions.Add("parameters", parameterList);

            return Create(context, title, detail, extensions);
        }

        #endregion
    }
}
