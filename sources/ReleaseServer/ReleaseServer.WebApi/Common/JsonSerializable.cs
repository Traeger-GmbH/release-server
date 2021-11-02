//--------------------------------------------------------------------------------------------------
// <copyright file="JsonSerializable.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;
using System.IO.Compression;
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
            return FromString(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Retrieves the specified type from a Json file w
        /// </summary>
        /// <param name="zipArchiveEntry"></param>
        /// <returns>An object of the specified type.</returns>
        public static T FromJsonFile(ZipArchiveEntry zipArchiveEntry)
        {
            using var streamReader = new StreamReader(zipArchiveEntry.Open(), System.Text.Encoding.UTF8);
            var content = streamReader.ReadToEnd();
            return FromString(content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T FromString(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, JsonConfiguration.Settings);
        }

        /// <summary>
        /// Stores the object into a file under the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="formatting"></param>
        /// <returns>An object of the specified type.</returns>
        public void ToJsonFile(string path, Formatting formatting = Formatting.Indented)
        {
            using var streamWriter = new StreamWriter(path);
            var jsonString = JsonConvert.SerializeObject(this, formatting, JsonConfiguration.Settings);
            streamWriter.Write(jsonString);
        }
    }
}
