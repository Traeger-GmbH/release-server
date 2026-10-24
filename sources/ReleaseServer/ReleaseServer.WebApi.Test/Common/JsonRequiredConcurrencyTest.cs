//--------------------------------------------------------------------------------------------------
// <copyright file="JsonRequiredConcurrencyTest.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2026.
// </copyright>
//--------------------------------------------------------------------------------------------------

using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test
{
    /// <summary>
    /// Newtonsoft.Json initializes the check of required properties lazily and not thread-safe. Without
    /// <see cref="JsonConfiguration.PrepareContracts"/>, about 1 of 2500 concurrent first deserializations
    /// skipped the [JsonRequired] check (seen as a sporadically failing upload validation test in CI).
    /// </summary>
    public class JsonRequiredConcurrencyTest
    {
        [Fact]
        public void RequiredProperties_AreAlwaysCheckedUnderConcurrency()
        {
            JsonConfiguration.PrepareContracts();
            const string json = "{ \"ArtifactFilename\": \"artifact.zip\" }";
            var accepted = 0;

            Parallel.For(0, 20000, new ParallelOptions { MaxDegreeOfParallelism = 16 }, _ =>
            {
                try
                {
                    new JsonSerializer().Deserialize(new StringReader(json), typeof(DeploymentMetaInformation));
                    Interlocked.Increment(ref accepted);
                }
                catch (JsonSerializationException)
                {
                    // Expected: ReleaseNotesFileName is missing
                }
            });

            Assert.Equal(0, accepted);
        }
    }
}
