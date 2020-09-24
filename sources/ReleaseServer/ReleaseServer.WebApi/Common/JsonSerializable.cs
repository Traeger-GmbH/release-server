//--------------------------------------------------------------------------------------------------
// <copyright file="JsonSerializable.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi
{
    /// <summary>
    /// Provides methods for deserializing Json sources to the specified Type>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class JsonSerializable<T>
    {
        /// <summary>
        /// Retrieves the specified type from a Json file w
        /// </summary>
        /// <param name="filePath">The path to the Json file.</param>
        /// <returns>An object of the specified type.</returns>
        public static T FromJsonFile(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), JsonConfiguration.Settings);
        }
    }
}