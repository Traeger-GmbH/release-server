//--------------------------------------------------------------------------------------------------
// <copyright file="BasicAuthOperationsFilter.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ReleaseServer.WebApi
{
    internal class BasicAuthOperationsFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var noAuthRequired = context.ApiDescription.CustomAttributes().Any(attr => attr.GetType() == typeof(AllowAnonymousAttribute));

            if (noAuthRequired) return;

            // Make the authentication requirement visible in the operation list and in the details.
            operation.Summary = (operation.Summary ?? "").TrimEnd('.') + " (requires authentication)";
            operation.Description = "**Requires authentication** (HTTP Basic). Not intended for public use.\n\n" + operation.Description;

            operation.Responses ??= new OpenApiResponses();
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", new OpenApiResponse { Description = "Authentication is missing or the credentials are wrong." });
            }

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("basic", context.Document)] = new List<string>()
                }
            };
        }
    }
}