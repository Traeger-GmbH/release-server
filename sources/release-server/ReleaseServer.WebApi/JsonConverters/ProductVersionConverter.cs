//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionConverter.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Models;


namespace ReleaseServer.WebApi.JsonConverters
{
    public class ProductVersionConverter : JsonConverter<ProductVersion>
    {
        public override void WriteJson(JsonWriter writer, ProductVersion value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override ProductVersion ReadJson(JsonReader reader, Type objectType, ProductVersion existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("this functionality is not implemented yet");
        }
    }
}