using System;
using System.Collections.Generic;
using System.Linq;
using ReleaseServer.WebApi.Mappers;
using ReleaseServer.WebApi.Models;
using Xunit;

namespace ReleaseServer.WebApi.Test
{
    public class ProductVersionComparisonTest
    {
        private readonly List<ProductVersion> versionList;

        public ProductVersionComparisonTest()
        {
            versionList = new List<ProductVersion>
            {
                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "alpha1"
                },
                
                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "rc1"
                },

                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "beta"
                },
                
                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "alpha2"
                },

                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "aaa"
                },

                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "open"
                },

                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "rc"
                },

                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = "zzz"
                }, 
                
                new ProductVersion
                {
                    VersionNumber = new Version("1.0"),
                    VersionSuffix = ""
                }, 
                
                new ProductVersion
                {
                    VersionNumber = new Version("1.1"),
                    VersionSuffix = ""
                },
            };
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
            Assert.Equal(0, versionList[8].CompareTo("1.0".ToProductVersion()));
            
            //1.0-alpha1 = 1.0-alpha-1
            Assert.Equal(0, versionList[0].CompareTo("1.0-alpha1".ToProductVersion()));
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
            var testVersion = "1.0".ToProductVersion();
            
            //1.0 is equal 1.0
            Assert.True(versionList[8].Equals(testVersion));
            
            //1.0 is not equal to 1.0-alpha1
            Assert.False(versionList[0].Equals(testVersion));
            
            //1.0 is not equal to 1.1
            Assert.False(versionList[9].Equals(testVersion));
        }

        [Fact]
        public void TestOperators()
        {
            var testVersion = "1.0".ToProductVersion();
            
            //1.0 is equal 1.0
            Assert.True(versionList[8]==(testVersion));
            Assert.False(versionList[8]!=(testVersion));
            
            //1.0 is not equal to 1.0-alpha1
            Assert.True(versionList[0]!=(testVersion));
            Assert.False(versionList[0]==(testVersion));
            
            //1.0 is not equal to 1.1
            Assert.True(versionList[9]!=(testVersion));
            Assert.False(versionList[9]==(testVersion));
        }
    }
}