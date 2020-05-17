using System.Collections.Generic;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ProductInformationResponseMapperTest
    {
        private readonly ProductInformationModel testProductInfo1, testProductInfo2;

        public ProductInformationResponseMapperTest()
        {
            testProductInfo1 = new ProductInformationModel
            {
                ProductIdentifier = "product1",
                Os = "ubuntu",
                HwArchitecture = "arm64",
                Version = "1.0".ToProductVersion()
            };

            testProductInfo2 = new ProductInformationModel
            {
                ProductIdentifier = "product1",
                Os = "ubuntu",
                HwArchitecture = "amd64",
                Version = "1.1-beta".ToProductVersion()
            };
        }

        [Fact]
        public void TestProductInformationResponseMapping()
        {
            //Setup
            var expectedProductResult1 = new ProductInformationResponseModel
            {
                Identifier = "product1",
                Os = "ubuntu",
                Architecture = "arm64",
                Version = "1.0",
                ReleaseNotes = new Dictionary<string, List<ChangeSet>>()
            };
            
            var expectedProductResult2 = new ProductInformationResponseModel
            {
                Identifier = "product1",
                Os = "ubuntu",
                Architecture = "amd64",
                Version = "1.1-beta",
                ReleaseNotes = new Dictionary<string, List<ChangeSet>>()
            };

            //Act
            var testProductResult1 = testProductInfo1.ToProductInfoResponse();
            var testProductResult2 = testProductInfo2.ToProductInfoResponse();
            
            //Assert
            testProductResult1.Should().BeEquivalentTo(expectedProductResult1);
            testProductResult2.Should().BeEquivalentTo(expectedProductResult2);
        }
        
        [Fact]
        public void TestProductInformationListResponseMapping()
        {
            //Setup
            var testProductInfoList = new List<ProductInformationModel>
            {
                testProductInfo1,
                testProductInfo2
            };


            var expectedProductInforListRepsonse = new ProductInformationListResponseModel
            {
                ProductInformation = new List<ProductInformationResponseModel>
                {
                    new ProductInformationResponseModel
                    {
                        Identifier = "product1",
                        Os = "ubuntu",
                        Architecture = "arm64",
                        Version = "1.0",
                        ReleaseNotes = new Dictionary<string, List<ChangeSet>>()
                    },

                    new ProductInformationResponseModel
                    {
                        Identifier = "product1",
                        Os = "ubuntu",
                        Architecture = "amd64",
                        Version = "1.1-beta",
                        ReleaseNotes = new Dictionary<string, List<ChangeSet>>()
                    }
                }
            };

            //Act
            var testResponse = testProductInfoList.ToProductInfoListResponse();
            
            //Assert
            testResponse.Should().BeEquivalentTo(expectedProductInforListRepsonse);
        }
    }
}