using System;
using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ProductInformationMapperTest
    {
        private readonly string validTestPath1, validTestPath2, validTestPath3;
        private readonly string invalidTestPath;
        private readonly ProductInformationModel expectedProductInfo;
        
        public ProductInformationMapperTest()
        {
            //Setup
            validTestPath1 = Path.Combine("./", "productx", "debian", "amd64", "1.0");
            validTestPath2 = Path.Combine("this/is/a/root/path", "productx", "debian", "amd64", "1.0");
            validTestPath3 = Path.Combine(".", "productx", "debian", "amd64", "1.0");
           
            invalidTestPath = Path.Combine("./", "debian", "amd64", "1.0");
            
            expectedProductInfo = new ProductInformationModel
            {
                ProductIdentifier = "productx",
                Os = "debian",
                HwArchitecture = "amd64",
                Version = "1.0".ToProductVersion(),
            };
        }
        
        [Fact]
        public void ConvertValidPathToProductInfo()
        {
            var testProductInfo1 = ProductInformationMapper.PathToProductInfo("./", validTestPath1);
            var testProductInfo2 = ProductInformationMapper.PathToProductInfo("this/is/a/root/path", validTestPath2);
            var testProductInfo3 = ProductInformationMapper.PathToProductInfo(".", validTestPath3);
            
            
            testProductInfo1.Should().BeEquivalentTo(expectedProductInfo);
            testProductInfo2.Should().BeEquivalentTo(expectedProductInfo);
            testProductInfo3.Should().BeEquivalentTo(expectedProductInfo);
        }
        
        [Fact]
        public void ConvertInvalidPathToProductInfo()
        {
            var testProductInfo = ProductInformationMapper.PathToProductInfo("./", invalidTestPath);
            
            Assert.Null(testProductInfo);
        }
    }
}