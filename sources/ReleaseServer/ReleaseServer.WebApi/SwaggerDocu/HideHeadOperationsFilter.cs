//--------------------------------------------------------------------------------------------------
// <copyright file="HideHeadOperationsFilter.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Net.Http;

using Microsoft.OpenApi;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Every public GET endpoint also answers HEAD (same status and headers, no body). The HEAD variants are
    /// not listed as separate operations, so the documented API stays the same; the API description
    /// mentions the HEAD support.
    /// </summary>
    internal class HideHeadOperationsFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument document, DocumentFilterContext context)
        {
            foreach (var path in document.Paths.Values)
            {
                path.Operations?.Remove(HttpMethod.Head);
            }
        }
    }
}
