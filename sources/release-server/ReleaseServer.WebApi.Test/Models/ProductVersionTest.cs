//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersionTest.cs" company="Traeger IndustryComponents GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

using ReleaseServer.WebApi.Models;

using Xunit;

namespace ReleaseServer.WebApi.Test.Models
{
    public class ProductVersionTest
    {
        private readonly List<ProductVersion> versionList;

        public ProductVersionTest()
        {
            versionList = new List<ProductVersion>
            {
                new ProductVersion("1.0-alpha1"),
                new ProductVersion("1.0-rc1"),
                new ProductVersion("1.0-beta"),
                new ProductVersion("1.0-alpha2"),
                new ProductVersion("1.0-aaa"),
                new ProductVersion("1.0-open"),
                new ProductVersion("1.0-rc"),
                new ProductVersion("1.0-zzz"), 
                new ProductVersion("1.0"),
                new ProductVersion("1.1")
            };
        }

        [Fact]
        public void ProductVersionCreation_Success()
        {
            var testVersion1 = new ProductVersion("1.0");

            Assert.Equal(new Version("1.0"), testVersion1.VersionNumber);
            Assert.Equal("", testVersion1.VersionSuffix);
            
            var testVersion2 = new ProductVersion("1.1-rc2");
            Assert.Equal(new Version("1.1"), testVersion2.VersionNumber);
            Assert.Equal("rc2", testVersion2.VersionSuffix);
        }
        
        [Fact]
        public void ProductVersionCreation_Error()
        {
            Assert.Throws<ArgumentException>(() => new ProductVersion("invalid"));
        }

        [Fact]
        public void CompareProductVersions()
        {
            //alpha1 < alpha2
            Assert.Equal(-1, versionList[0].CompareTo(versionList[3]));
            
            //alpha2 < beta
            Assert.Equal(-1, versionList[3].CompareTo(versionList[2]));
            
            //Version with no suffix always > as without suffix (if they have the same version number)
            Assert.Equal(1, versionList[8].CompareTo(versionList[1]));
            
            //1.1 is always > than all 1.0 versions (independent of suffix)
            Assert.Equal(1, versionList[9].CompareTo(versionList[1]));
            
            //1.0 = 1.0
            Assert.Equal(0, versionList[8].CompareTo(new ProductVersion("1.0")));
            
            //1.0-alpha1 = 1.0-alpha-1
            Assert.Equal(0, versionList[0].CompareTo(new ProductVersion("1.0-alpha1")));
        }

        [Fact]
        public void OrderProductVersionsByDescending()
        {
            //Has to be sorted like specified in the microsoft specification for nuget packages
            var expectedList = new List<string> {
                "1.1",
                "1.0",
                "1.0-zzz",
                "1.0-rc1",
                "1.0-rc",
                "1.0-open",
                "1.0-beta",
                "1.0-alpha2",
                "1.0-alpha1",
                "1.0-aaa"
            };
            
            var sortedVersionList = versionList.OrderByDescending(v => v).ToList().ConvertAll(v => v.ToString());

            Assert.Equal(sortedVersionList, expectedList);
        }

        [Fact]
        public void TestEqual()
        {
            var testVersion = new ProductVersion("1.0");
            
            //1.0 is equal 1.0
            Assert.True(versionList[8].Equals(testVersion));
            
            //1.0 is not equal to 1.0-alpha1
            Assert.False(versionList[0].Equals(testVersion));
            
            //1.0 is not equal to 1.1
            Assert.False(versionList[9].Equals(testVersion));
        }
        
        [Fact]
        public void TestToString()
        {
            //Prepare
            var testVersion1 = new ProductVersion("1.0");
            var testVersion2 = new ProductVersion("1.0-alpha1");
            
            //Act
            var testVersion1String = testVersion1.ToString();
            var testVersion2String = testVersion2.ToString();
            
            //Assert
            Assert.Equal("1.0", testVersion1String);
            Assert.Equal("1.0-alpha1", testVersion2String);
        }
    }
}