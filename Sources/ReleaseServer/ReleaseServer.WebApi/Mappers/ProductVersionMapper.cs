using System;
using System.Collections.Generic;
using ReleaseServer.WebApi.Models;

namespace ReleaseServer.WebApi.Mappers
{
    public static class ProductVersionMapper
    {
        public static ProductVersion ToProductVersion(this string version)
        {
            var versionElements = version.Split("-", 2);
            
            if (versionElements.Length == 1)
            {
                return new ProductVersion
                {
                    VersionNumber = new Version(versionElements[0]),
                    VersionSuffix = ""
                };
            }

            return new ProductVersion
            {
                VersionNumber = new Version(versionElements[0]),
                VersionSuffix = versionElements[1]
            };
        }

        public static ProductVersionResponseModel ToProductVersionResponse(this string version)
        {
            return new ProductVersionResponseModel
            {
                Version = version
            };
        }
        
        public static ProductVersionListResponseModel ToProductVersionListResponse(this List<string> versionList)
        {
            return new ProductVersionListResponseModel
            {
                Versions = versionList
            };
        }
    }
}