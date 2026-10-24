//--------------------------------------------------------------------------------------------------
// <copyright file="Golden.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Xunit;

namespace ReleaseServer.WebApi.Test.Integration
{
    /// <summary>
    /// Compares responses with recorded golden files in "Integration/Golden". The golden files were
    /// recorded with the .NET Core 3.1 release server and pin the public API: property names, property
    /// order, value formats and status codes must stay the same.
    /// Set the environment variable RELEASE_SERVER_UPDATE_GOLDEN=1 to (re)record the files.
    /// </summary>
    public static class Golden
    {
        private static bool UpdateMode =>
            Environment.GetEnvironmentVariable("RELEASE_SERVER_UPDATE_GOLDEN") == "1";

        private static string GoldenDirectory =>
            Path.Combine(TestUtils.GetProjectDirectory(), "Integration", "Golden");

        /// <summary>
        /// Compares a JSON response body (normalized) with the golden file of the given name.
        /// </summary>
        public static void AssertJson(string name, string actualBody)
        {
            AssertText(name + ".json", NormalizeJson(actualBody));
        }

        /// <summary>
        /// Compares a text with the golden file of the given name.
        /// </summary>
        public static void AssertText(string fileName, string actual)
        {
            var path = Path.Combine(GoldenDirectory, fileName);
            actual = actual.Replace("\r\n", "\n");

            if (UpdateMode)
            {
                Directory.CreateDirectory(GoldenDirectory);
                File.WriteAllText(path, actual);
                return;
            }

            Assert.True(File.Exists(path), $"Golden file '{fileName}' is missing. Record it with RELEASE_SERVER_UPDATE_GOLDEN=1.");
            var expected = File.ReadAllText(path).Replace("\r\n", "\n");
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Normalizes a JSON body: values that change with every request (traceId) are replaced and arrays
        /// whose order depends on the file system enumeration ("products", "platforms") are sorted.
        /// The property order is kept, because clients may depend on it.
        /// </summary>
        public static string NormalizeJson(string body)
        {
            // Keep date strings untouched, so the golden files pin their exact format.
            using var reader = new JsonTextReader(new StringReader(body)) { DateParseHandling = DateParseHandling.None };
            var token = JToken.ReadFrom(reader);
            Normalize(token);
            return token.ToString(Formatting.Indented);
        }

        private static void Normalize(JToken token)
        {
            switch (token)
            {
                case JObject obj:
                    foreach (var property in obj.Properties().ToList())
                    {
                        if (property.Name == "traceId")
                        {
                            property.Value = "<traceId>";
                        }
                        else if ((property.Name == "products" || property.Name == "platforms") && property.Value is JArray array
                            && array.All(item => item.Type == JTokenType.String))
                        {
                            property.Value = new JArray(array.Select(item => (string)item).OrderBy(item => item, StringComparer.Ordinal));
                        }
                        else
                        {
                            Normalize(property.Value);
                        }
                    }
                    break;

                case JArray array:
                    foreach (var item in array)
                    {
                        Normalize(item);
                    }
                    break;
            }
        }
    }
}
