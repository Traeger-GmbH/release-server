//--------------------------------------------------------------------------------------------------
// <copyright file="ArtifactHashes.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// SHA-256 checksums of artifact files.
    /// </summary>
    public static class ArtifactHashes
    {
        /// <summary>
        /// Copies a stream and returns the SHA-256 checksum of the copied data as lower case hex string.
        /// </summary>
        public static string CopyAndHash(Stream source, Stream destination)
        {
            using var sha256 = SHA256.Create();
            using (var hashingStream = new CryptoStream(destination, sha256, CryptoStreamMode.Write, leaveOpen: true))
            {
                source.CopyTo(hashingStream);
                hashingStream.FlushFinalBlock();
            }

            return Convert.ToHexString(sha256.Hash).ToLowerInvariant();
        }

        /// <summary>
        /// Checks whether a value is a SHA-256 checksum in hex notation.
        /// </summary>
        public static bool IsValid(string value)
        {
            return value != null && value.Length == 64 && value.All(Uri.IsHexDigit);
        }

        /// <summary>
        /// Adds the checksum to an existing deployment.json. All existing properties and their values are kept;
        /// the new property follows the naming style of the file ("Sha256" for files written by the server,
        /// "sha256" for camelCase files). The file is replaced atomically.
        /// </summary>
        public static void AddToDeploymentFile(string deploymentFile, string sha256)
        {
            JObject json;
            using (var reader = new JsonTextReader(new StringReader(File.ReadAllText(deploymentFile))) { DateParseHandling = DateParseHandling.None })
            {
                json = JObject.Load(reader);
            }

            var pascalCase = json.Properties().Any(property => property.Name.Length > 0 && char.IsUpper(property.Name[0]));
            json[pascalCase ? "Sha256" : "sha256"] = sha256;

            var temporaryFile = Path.Combine(Path.GetDirectoryName(deploymentFile), $".deployment.json.{Guid.NewGuid():N}.tmp");
            try
            {
                File.WriteAllText(temporaryFile, json.ToString(Formatting.Indented), new UTF8Encoding(false));
                File.Move(temporaryFile, deploymentFile, true);
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }
    }
}
