//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionConverterTest.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test.JsonConverters
{
    public class ProductVersionConverterTest
    {
        [Fact]
        public void TestWriteJson_Success()
        {
            //Setup
            var testProductVersion = new ProductVersion("1.8-rc2");

            //Act
            var productVersionValue = JsonConvert.SerializeObject(testProductVersion, JsonConfiguration.Settings);

            //Assert
            Assert.Equal("\"1.8-rc2\"", productVersionValue);
        }

        [Fact]
        public void TestReadJson_NotImplementedException()
        {
            //Setup
            var emptyTestJson = "{}";

           Assert.Throws<NotImplementedException>(() => JsonConvert.DeserializeObject<ProductVersion>(emptyTestJson, JsonConfiguration.Settings));
        }
    }
}