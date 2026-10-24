//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactContentTypeProvider.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using Microsoft.AspNetCore.StaticFiles;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Determines the content type of downloaded files. The mapping is kept identical to the one of
    /// ASP.NET Core 3.1, because the Content-Type header of downloads is part of the public API.
    /// </summary>
    internal static class ArtifactContentTypeProvider
    {
        private const string DefaultContentType = "application/octet-stream";

        private static readonly FileExtensionContentTypeProvider Provider = CreateProvider();

        /// <summary>
        /// Gets the content type for the given file name (application/octet-stream for unknown extensions).
        /// </summary>
        public static string GetContentType(string fileName)
        {
            return Provider.TryGetContentType(fileName, out var contentType) ? contentType : DefaultContentType;
        }

        private static FileExtensionContentTypeProvider CreateProvider()
        {
            var provider = new FileExtensionContentTypeProvider();

            // Revert the mappings that changed after ASP.NET Core 3.1.
            provider.Mappings.Remove(".avif");
            provider.Mappings.Remove(".exe");
            provider.Mappings.Remove(".mjs");
            provider.Mappings.Remove(".webmanifest");
            provider.Mappings[".csv"] = DefaultContentType;
            provider.Mappings[".js"] = "application/javascript";

            return provider;
        }
    }
}
