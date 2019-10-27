using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace release_server_web_api_test
{
    public class ProductInformationMapperTest
    {
        private readonly string validTestPath;
        private readonly string invalidTestPath;
        private readonly ProductInformationModel expectedProductInfo;
        
        public ProductInformationMapperTest()
        {
            //Setup
            validTestPath = Path.Combine(".", "productx", "debian", "amd64", "1.0");
           
            invalidTestPath = Path.Combine(".", "debian", "amd64", "1.0");

            
            expectedProductInfo = new ProductInformationModel
            {
                ProductIdentifier = "productx",
                Os = "debian",
                HwArchitecture = "amd64",
                Version = "1.0",
            };
        }
        
        [Fact]
        public void ConvertValidPathToProductInfo()
        {
            var testProductInfo = ProductInformationMapper.PathToProductInfo(validTestPath);
            
            testProductInfo.Should().BeEquivalentTo(expectedProductInfo);
        }
        
        [Fact]
        public void ConvertInvalidPathToProductInfo()
        {
            var testProductInfo = ProductInformationMapper.PathToProductInfo(invalidTestPath);
            
            Assert.Null(testProductInfo);
        }
    }
}