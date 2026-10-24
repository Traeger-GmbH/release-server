//--------------------------------------------------------------------------------------------------
// <copyright file="CorsPolicies.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// CORS policies of the release server.
    /// The public (anonymous) endpoints can be read by any website, like before. The authenticated
    /// endpoints can only be called by browsers from the origins configured in "Cors:WriteOrigins"
    /// (none by default). Clients outside of a browser are not affected by CORS.
    /// </summary>
    internal static class CorsPolicies
    {
        /// <summary>
        /// The name of the policy for authenticated endpoints.
        /// </summary>
        public const string Authenticated = "Authenticated";

        private static readonly TimeSpan PreflightMaxAge = TimeSpan.FromSeconds(1728000);

        public static void Configure(CorsOptions options, IConfiguration configuration)
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                    .WithMethods("GET", "HEAD")
                    .SetIsOriginAllowed(_ => true)
                    .SetPreflightMaxAge(PreflightMaxAge);
            });

            var writeOrigins = configuration.GetSection("Cors:WriteOrigins").Get<string[]>() ?? Array.Empty<string>();
            writeOrigins = writeOrigins.Where(origin => !string.IsNullOrWhiteSpace(origin)).Select(origin => origin.Trim().TrimEnd('/')).ToArray();

            options.AddPolicy(Authenticated, builder =>
            {
                builder
                    .WithOrigins(writeOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetPreflightMaxAge(PreflightMaxAge);
            });
        }
    }
}
