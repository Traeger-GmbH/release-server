//--------------------------------------------------------------------------------------------------
// <copyright file="LegacyProblemDetails.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Hellang.Middleware.ProblemDetails;

using Microsoft.AspNetCore.Mvc;

using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Keeps error responses (problem details) identical to the release server based on ASP.NET Core 3.1.
    /// Newer framework and library versions changed the "type" links and the request matching of
    /// actions with uploaded files.
    /// </summary>
    internal static class LegacyProblemDetails
    {
        private const string CurrentStatusLinkPrefix = "https://httpstatuses.io/";
        private const string LegacyStatusLinkPrefix = "https://httpstatuses.com/";

        public static void Configure(ApiBehaviorOptions options)
        {
            // ASP.NET Core 7+ only selects actions with IFormFile parameters for multipart requests.
            // Requests without a form body must reach the action (and its authorization) like before.
            options.SuppressConsumesConstraintForFormFileParameters = true;

            SetLink(options, 400, "https://tools.ietf.org/html/rfc7231#section-6.5.1");
            SetLink(options, 401, "https://tools.ietf.org/html/rfc7235#section-3.1");
            SetLink(options, 403, "https://tools.ietf.org/html/rfc7231#section-6.5.3");
            SetLink(options, 404, "https://tools.ietf.org/html/rfc7231#section-6.5.4");
            SetLink(options, 406, "https://tools.ietf.org/html/rfc7231#section-6.5.6");
            SetLink(options, 409, "https://tools.ietf.org/html/rfc7231#section-6.5.8");
            SetLink(options, 415, "https://tools.ietf.org/html/rfc7231#section-6.5.13");
            SetLink(options, 422, "https://tools.ietf.org/html/rfc4918#section-11.2");
            SetLink(options, 500, "https://tools.ietf.org/html/rfc7231#section-6.6.1");
        }

        public static void Configure(ProblemDetailsOptions options)
        {
            options.OnBeforeWriteDetails = (_, details) =>
            {
                if (details.Type != null && details.Type.StartsWith(CurrentStatusLinkPrefix))
                {
                    details.Type = LegacyStatusLinkPrefix + details.Type.Substring(CurrentStatusLinkPrefix.Length);
                }
            };
        }

        private static void SetLink(ApiBehaviorOptions options, int statusCode, string link)
        {
            if (options.ClientErrorMapping.TryGetValue(statusCode, out var data))
            {
                data.Link = link;
            }
        }
    }
}
