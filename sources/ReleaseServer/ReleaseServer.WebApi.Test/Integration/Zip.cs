//--------------------------------------------------------------------------------------------------
// <copyright file="Zip.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Helpers to build ZIP payloads and multipart requests for the upload endpoints.
    /// </summary>
    public static class Zip
    {
        public static byte[] Create(IDictionary<string, byte[]> entries)
        {
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                foreach (var (name, content) in entries)
                {
                    var entry = archive.CreateEntry(name);
                    using var entryStream = entry.Open();
                    entryStream.Write(content, 0, content.Length);
                }
            }

            return memoryStream.ToArray();
        }

        public static byte[] Create(IDictionary<string, string> entries)
        {
            return Create(entries.ToDictionary(entry => entry.Key, entry => Encoding.UTF8.GetBytes(entry.Value)));
        }

        public static SortedDictionary<string, byte[]> Read(byte[] zip)
        {
            var result = new SortedDictionary<string, byte[]>(System.StringComparer.Ordinal);
            using var archive = new ZipArchive(new MemoryStream(zip), ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("/") || entry.FullName.EndsWith("\\"))
                {
                    // Directory entry (only written for empty directories).
                    result[entry.FullName.Replace('\\', '/')] = new byte[0];
                    continue;
                }

                using var entryStream = entry.Open();
                using var buffer = new MemoryStream();
                entryStream.CopyTo(buffer);
                result[entry.FullName.Replace('\\', '/')] = buffer.ToArray();
            }

            return result;
        }

        public static MultipartFormDataContent CreateFormContent(string fieldName, byte[] content, string fileName,
            string contentType = "application/zip")
        {
            var fileContent = new ByteArrayContent(content);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            var form = new MultipartFormDataContent();
            form.Add(fileContent, fieldName, fileName);
            return form;
        }

        /// <summary>
        /// Builds a single artifact payload (deployment.json, release notes and artifact) as used by
        /// PUT /artifacts/{product}/{os}/{architecture}/{version}.
        /// </summary>
        public static byte[] CreateArtifactPayload(string artifactFileName, byte[] artifact, string releaseNotes)
        {
            return Create(new Dictionary<string, byte[]>
            {
                ["deployment.json"] = Encoding.UTF8.GetBytes(LegacyStore.DeploymentCamelCase.Replace("{0}", artifactFileName)),
                ["releaseNotes.json"] = Encoding.UTF8.GetBytes(releaseNotes),
                [artifactFileName] = artifact,
            });
        }

        /// <summary>
        /// Builds a package payload (package.json plus one file per platform) as used by PUT /artifacts.
        /// </summary>
        public static byte[] CreatePackagePayload(string identifier, string version, IDictionary<string, string> platformFiles)
        {
            var root = $"{identifier}-v{version}";
            var deployments = string.Join(",\n    ", platformFiles.Select(file => $"\"{file.Key}\": \"files/{file.Value}\""));
            var packageJson = $@"{{
  ""identifier"": ""{identifier}"",
  ""version"": ""{version}"",
  ""releaseNotes"": {{
    ""releaseDate"": ""2021-10-28"",
    ""isPreviewRelease"": true,
    ""IsSecurityPatch"": false,
    ""changes"": {{
      ""de"": [{{ ""added"": [""Neu""], ""fixed"": [], ""breaking"": [], ""deprecated"": [] }}],
      ""en"": [{{ ""added"": [""New""], ""fixed"": [], ""breaking"": [], ""deprecated"": [] }}]
    }}
  }},
  ""deployments"": {{
    {deployments}
  }}
}}";

            var entries = new Dictionary<string, byte[]>
            {
                [$"{root}/package.json"] = Encoding.UTF8.GetBytes(packageJson),
            };

            foreach (var (platform, fileName) in platformFiles)
            {
                entries[$"{root}/files/{fileName}"] = LegacyStore.ArtifactContent($"{identifier}/{platform}/{version}");
            }

            return Create(entries);
        }
    }
}
