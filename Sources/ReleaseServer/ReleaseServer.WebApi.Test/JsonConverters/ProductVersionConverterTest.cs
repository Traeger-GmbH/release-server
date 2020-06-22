using System;
using System.IO;
using Newtonsoft.Json;
using ReleaseServer.WebApi.Config;
using ReleaseServer.WebApi.JsonConverters;
using ReleaseServer.WebApi.Models;
using ReleaseServer.WebApi.Test.Utils;
using Xunit;

namespace ReleaseServer.WebApi.Test.JsonConverters
{
    public class ProductVersionConverterTest
    {

        private readonly string ProjectDirectory = TestUtils.GetProjectDirectory();
        
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