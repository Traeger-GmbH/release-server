//--------------------------------------------------------------------------------------------------
// <copyright file="NuGetPackageIndex.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using Microsoft.Extensions.Logging;

using NuGet.Packaging;
using NuGet.Versioning;

namespace ReleaseServer.WebApi.NuGet
{
    /// <summary>
    /// A NuGet package stored as artifact of a release.
    /// </summary>
    public class NuGetPackage
    {
        /// <summary>Gets or sets the package ID from the .nuspec.</summary>
        public string Id { get; set; }

        /// <summary>Gets or sets the package version from the .nuspec.</summary>
        public NuGetVersion Version { get; set; }

        /// <summary>Gets or sets the full path of the .nupkg file.</summary>
        public string FilePath { get; set; }

        /// <summary>Gets or sets the content of the .nuspec file.</summary>
        public byte[] Nuspec { get; set; }

        /// <summary>Gets or sets the metadata read from the .nuspec.</summary>
        public NuspecReader Metadata { get; set; }

        /// <summary>Gets or sets the release date of the release the package belongs to.</summary>
        public DateTime Published { get; set; }
    }

    /// <summary>
    /// Finds all NuGet packages (.nupkg artifacts) in the artifact directory. ID and version come from the
    /// .nuspec inside the package, not from the product name. Packages are read once and cached as long as
    /// their file does not change.
    /// </summary>
    public class NuGetPackageIndex
    {
        private readonly IReleaseArtifactService releaseArtifactService;
        private readonly ILogger<NuGetPackageIndex> logger;
        private readonly object cacheLock = new object();
        private Dictionary<string, (long Length, DateTime LastWrite, NuGetPackage Package)> cache =
            new Dictionary<string, (long, DateTime, NuGetPackage)>(StringComparer.Ordinal);

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetPackageIndex"/> class.
        /// </summary>
        public NuGetPackageIndex(IReleaseArtifactService releaseArtifactService, ILogger<NuGetPackageIndex> logger)
        {
            this.releaseArtifactService = releaseArtifactService;
            this.logger = logger;
        }

        /// <summary>
        /// Gets all packages. If the same ID and version is stored more than once, the first one is used.
        /// </summary>
        public async Task<IReadOnlyList<NuGetPackage>> GetPackages()
        {
            var files = new List<(string Path, DateTime Published)>();
            foreach (var product in await releaseArtifactService.GetProductList())
            {
                foreach (var deployment in await releaseArtifactService.GetDeploymentInformations(product))
                {
                    if (deployment.ArtifactFilePath != null
                        && deployment.ArtifactFileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase)
                        && File.Exists(deployment.ArtifactFilePath))
                    {
                        files.Add((deployment.ArtifactFilePath, deployment.ReleaseNotes.ReleaseDate));
                    }
                }
            }

            var packages = new List<NuGetPackage>();
            var updatedCache = new Dictionary<string, (long, DateTime, NuGetPackage)>(StringComparer.Ordinal);

            foreach (var (path, published) in files.OrderBy(file => file.Path, StringComparer.Ordinal))
            {
                var info = new FileInfo(path);
                NuGetPackage package;

                lock (cacheLock)
                {
                    package = cache.TryGetValue(path, out var cached) && cached.Length == info.Length && cached.LastWrite == info.LastWriteTimeUtc
                        ? cached.Package
                        : null;
                }

                package ??= Read(path);
                if (package == null)
                    continue;

                package.Published = published;
                updatedCache[path] = (info.Length, info.LastWriteTimeUtc, package);

                if (packages.Any(existing => existing.Id.Equals(package.Id, StringComparison.OrdinalIgnoreCase) && existing.Version == package.Version))
                {
                    logger.LogWarning("The NuGet package {0} {1} is stored more than once; {2} is ignored.", package.Id, package.Version, path);
                    continue;
                }

                packages.Add(package);
            }

            lock (cacheLock)
            {
                cache = updatedCache;
            }

            return packages;
        }

        private NuGetPackage Read(string path)
        {
            try
            {
                using var archive = ZipFile.OpenRead(path);
                var nuspecEntry = archive.Entries.SingleOrDefault(entry =>
                    !entry.FullName.Contains('/') && entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase));
                if (nuspecEntry == null)
                {
                    logger.LogWarning("{0} contains no .nuspec and is not offered in the NuGet feed.", path);
                    return null;
                }

                byte[] nuspec;
                using (var stream = nuspecEntry.Open())
                using (var buffer = new MemoryStream())
                {
                    stream.CopyTo(buffer);
                    nuspec = buffer.ToArray();
                }

                var reader = new NuspecReader(XDocument.Load(new MemoryStream(nuspec)));
                return new NuGetPackage
                {
                    Id = reader.GetId(),
                    Version = reader.GetVersion(),
                    FilePath = path,
                    Nuspec = nuspec,
                    Metadata = reader
                };
            }
            catch (Exception e)
            {
                logger.LogWarning("{0} is not a valid NuGet package and is not offered in the NuGet feed: {1}", path, e.Message);
                return null;
            }
        }
    }
}
