//--------------------------------------------------------------------------------------------------
// <copyright file="PlatformConverter.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2021.
// </copyright>
// <author>Fabian Träger</author>
//--------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;

using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi
{
    internal class PlatformConverter : JsonConverter<Platform>
    {
        public override void WriteJson(JsonWriter writer, Platform value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override Platform ReadJson(JsonReader reader, Type objectType, Platform existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return Platform.Parse(value);
        }
    }
}