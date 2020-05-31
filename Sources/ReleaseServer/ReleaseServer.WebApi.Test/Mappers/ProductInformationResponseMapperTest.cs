using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ProductInformationResponseMapperTest
    {
        private readonly ProductInformation testProductInfo1, testProductInfo2;

        public ProductInformationResponseMapperTest()
        {
            testProductInfo1 = new ProductInformation
            {
                ProductIdentifier = "product1",
                Os = "ubuntu",
                HwArchitecture = "arm64",
                Version = "1.0".ToProductVersion(),
                ReleaseNotes = new ReleaseNotes
                {
                    Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {
                            new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string> {"windows/any", "linux/rpi"},
                                    Added = new List<string> {"added de 1", "added de 2"},
                                    Fixed = new List<string> {"fix de 1", "fix de 2"},
                                    Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        }
                    }
                }
            };

            testProductInfo2 = new ProductInformation
            {
                ProductIdentifier = "product1",
                Os = "ubuntu",
                HwArchitecture = "amd64",
                Version = "1.1-beta".ToProductVersion(),
                ReleaseNotes = new ReleaseNotes
                {
                    Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {
                            new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string> {"windows/any"},
                                    Added = new List<string> {"added de 1", "added de 2"},
                                    Fixed = new List<string> {"fix de 1", "fix de 2"},
                                    Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        public void TestProductInformationResponseMapping()
        {
            //Setup
            var expectedProductResult1 = new ProductInformationResponse
            {
                Identifier = "product1",
                Os = "ubuntu",
                Architecture = "arm64",
                Version = "1.0",
                ReleaseNotes = new ReleaseNotes
                { Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {
                            new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string> {"windows/any", "linux/rpi"},
                                    Added = new List<string> {"added de 1", "added de 2"},
                                    Fixed = new List<string> {"fix de 1", "fix de 2"},
                                    Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        }
                    }
                }
            };
            
            var expectedProductResult2 = new ProductInformationResponse
            {
                Identifier = "product1",
                Os = "ubuntu",
                Architecture = "amd64",
                Version = "1.1-beta",
                ReleaseNotes = new ReleaseNotes
                { Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                    {
                        {
                            new CultureInfo("de"), new List<ChangeSet>
                            {
                                new ChangeSet
                                {
                                    Platforms = new List<string> {"windows/any"},
                                    Added = new List<string> {"added de 1", "added de 2"},
                                    Fixed = new List<string> {"fix de 1", "fix de 2"},
                                    Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                    Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                }
                            }
                        }
                    }
                }
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
            var testProductInfoList = new List<ProductInformation>
            {
                testProductInfo1,
                testProductInfo2
            };


            var expectedProductInfoListResponse = new ProductInformationListResponse
            {
                ProductInformation = new List<ProductInformationResponse>
                {
                    new ProductInformationResponse
                    {
                        Identifier = "product1",
                        Os = "ubuntu",
                        Architecture = "arm64",
                        Version = "1.0",
                        ReleaseNotes = new ReleaseNotes
                        { Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                            {
                                {
                                    new CultureInfo("de"), new List<ChangeSet>
                                    {
                                        new ChangeSet
                                        {
                                            Platforms = new List<string> {"windows/any", "linux/rpi"},
                                            Added = new List<string> {"added de 1", "added de 2"},
                                            Fixed = new List<string> {"fix de 1", "fix de 2"},
                                            Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                            Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                        }
                                    }
                                }
                            }
                        }
                    },

                    new ProductInformationResponse
                    {
                        Identifier = "product1",
                        Os = "ubuntu",
                        Architecture = "amd64",
                        Version = "1.1-beta",
                        ReleaseNotes = new ReleaseNotes
                        { Changes = new Dictionary<CultureInfo, List<ChangeSet>>
                            {
                                {
                                    new CultureInfo("de"), new List<ChangeSet>
                                    {
                                        new ChangeSet
                                        {
                                            Platforms = new List<string> {"windows/any"},
                                            Added = new List<string> {"added de 1", "added de 2"},
                                            Fixed = new List<string> {"fix de 1", "fix de 2"},
                                            Breaking = new List<string> {"breaking de 1", "breaking de 2"},
                                            Deprecated = new List<string> {"deprecated de 1", "deprecated de 2"}
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            //Act
            var testResponse = testProductInfoList.ToProductInfoListResponse();
            
            //Assert
            testResponse.Should().BeEquivalentTo(expectedProductInfoListResponse);
        }
    }
}