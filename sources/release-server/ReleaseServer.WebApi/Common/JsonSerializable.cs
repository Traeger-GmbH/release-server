//--------------------------------------------------------------------------------------------------
// <copyright file="JsonSerializable.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System.IO;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Config;

namespace ReleaseServer.WebApi.Common
{
    public class JsonSerializable<T>
    {
        public static T FromJsonFile(string filePath)
        {
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath), JsonConfiguration.Settings);
        }
    }
}