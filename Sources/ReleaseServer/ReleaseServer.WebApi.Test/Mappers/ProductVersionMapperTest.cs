using System;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ProductVersionMapperTest
    {
        private readonly ProductVersion expectedProductVersion1, expectedProductVersion2;
        
        public ProductVersionMapperTest()
        {
            expectedProductVersion1 = new ProductVersion
            {
                VersionNumber = new Version("1.0"),
                VersionSuffix = ""
            };

            expectedProductVersion2 = new ProductVersion
            {
                VersionNumber = new Version("1.1"),
                VersionSuffix = "rc2"
            };
        }

        [Fact]
        public void ConvertValidVersionToProductVersion()
        {
           var validVersion1 = "1.0";
           var validVersion2 = "1.1-rc2";

           var testProductVersion1 = validVersion1.ToProductVersion();
           var testProductVersion2 = validVersion2.ToProductVersion();
           
           Assert.Equal(expectedProductVersion1, testProductVersion1);
           Assert.Equal(expectedProductVersion2, testProductVersion2);
        }

        [Fact]
        public void ConvertInvalidVersionToProductVersion()
        {
            var invalidVersion = "invalid";
            
            Assert.Throws<ArgumentException>(() => invalidVersion.ToProductVersion());
        }
        
        
        
    }
}