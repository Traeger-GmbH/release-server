//--------------------------------------------------------------------------------------------------
// <copyright file="ProductInformationExample.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

using ReleaseServer.WebApi.Models;

using Swashbuckle.AspNetCore.Filters;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Example response of GET /artifacts/{product}/info for the Swagger documentation.
    /// </summary>
    internal class ProductInformationExample : IExamplesProvider<ProductInformation>
    {
        public ProductInformation GetExamples()
        {
            var preview = new ReleaseNotes
            {
                ReleaseDate = new DateTime(2026, 3, 2),
                IsPreviewRelease = true,
                Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                {
                    [new CultureInfo("en")] = new List<ChangeSet>
                    {
                        new ChangeSet { Added = new List<string> { "Support for ARM64 on Linux." } }
                    }
                }
            };

            var release = new ReleaseNotes
            {
                ReleaseDate = new DateTime(2026, 1, 31),
                IsSecurityPatch = true,
                Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                {
                    [new CultureInfo("de")] = new List<ChangeSet>
                    {
                        new ChangeSet { Fixed = new List<string> { "Sicherheitslücke in der Anmeldung behoben." } }
                    },
                    [new CultureInfo("en")] = new List<ChangeSet>
                    {
                        new ChangeSet
                        {
                            Platforms = new List<string> { "windows/x64" },
                            Fixed = new List<string> { "Fixed a security issue in the login." }
                        }
                    }
                }
            };

            var releases = new List<ReleaseInformation>
            {
                new ReleaseInformation(new ProductVersion("2.0.0-beta"), preview,
                    new[] { new Platform("windows", "x64"), new Platform("linux", "arm64") }),
                new ReleaseInformation(new ProductVersion("1.2.0"), release,
                    new[] { new Platform("windows", "x64"), new Platform("windows", "x86") }),
            };

            return new ProductInformation("example-product", releases, 50, 0);
        }
    }

    /// <summary>
    /// Example response of GET /artifacts for the Swagger documentation.
    /// </summary>
    internal class ProductListExample : IExamplesProvider<ProductList>
    {
        public ProductList GetExamples()
        {
            return new ProductList(new List<string> { "example-product", "another-product" });
        }
    }
}
