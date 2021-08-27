//--------------------------------------------------------------------------------------------------
// <copyright file="ProductVersion.cs" company="Traeger Industry Components GmbH">
//     This file is protected by Traeger Industry Components GmbH Copyright © 2019-2020.
// </copyright>
// <author>Timo Walter</author>
//--------------------------------------------------------------------------------------------------

using System;

namespace ReleaseServer.WebApi.Models
{
    /// <summary>
    /// Provides the information of an artifact product version and implements several operations for
    /// the <see cref="ProductVersion"/>.
    /// </summary>
    public class ProductVersion : IComparable<ProductVersion>
    {
        #region ---------- Public constructors ----------
        
        /// <summary>
        /// Initializes a new instance of the <see cref="ProductVersion"/> class.
        /// </summary>
        /// <param name="version">The product version of the artifact as string.</param>
        /// <remarks>The constructor separates the <see cref="VersionSuffix"/> from the <see cref="VersionNumber"/>.</remarks>
        public ProductVersion(string version)
        {
            var versionElements = version.Split("-", 2);
            
            if (versionElements.Length == 1)
            {
                VersionNumber = new Version(versionElements[0]);
                VersionSuffix = "";
            }
            else
            {
                VersionNumber = new Version(versionElements[0]);
                VersionSuffix = versionElements[1];
            }
        }
        
        #endregion
        
        #region ---------- Public properties ----------

        /// <summary>
        /// Gets or sets the version number of the artifact.
        /// </summary>
        /// <value>The version number of the artifact (e.g. 1.0, 1.1).This value is always set.</value>
        public Version VersionNumber { get; set; }

        /// <summary>
        /// Gets or sets the version suffix of the artifact (e.g. -alpha, -beta).
        /// </summary>
        /// <value>The version suffix of the artifact (e.g. -alpha, -beta). This value is not set, if the artifact version
        /// has no suffix.</value>
        public string VersionSuffix { get; set; }
        
        #endregion
        
        #region ---------- Public methods ----------

        /// <summary>
        /// Compares two instances of <see cref="ProductVersion"/>.
        /// </summary>
        /// <param name="other">The <see cref="ProductVersion"/> for comparison.</param>
        /// <returns> 1 (GreaterThan), 0 (Equal), -1 (LessThan)</returns>
        public int CompareTo(ProductVersion other)
        {
            var versionNumberComparison = VersionNumber.CompareTo(other.VersionNumber);

            //If the version numbers are equal
            if (versionNumberComparison == 0)
            {
                //Versions without a VersionSuffix do have a higher priority
                if (VersionSuffix == "" && other.VersionSuffix != "")
                    return 1;
                
                if (other.VersionSuffix == "" && VersionSuffix != "")
                    return -1;

                if (String.Compare(VersionSuffix, other.VersionSuffix, StringComparison.Ordinal) == 0)
                    return 0;
                
                if (String.Compare(VersionSuffix, other.VersionSuffix, StringComparison.Ordinal) < 0)
                    return -1;
                
                if (String.Compare(VersionSuffix, other.VersionSuffix, StringComparison.Ordinal) > 0)
                    return 1;
            }

            //The version numbers are not equal -> no comparison of the suffix necessary
            return versionNumberComparison;
        }
        
        /// <summary>
        /// Converts the <see cref="ProductVersion"/> to a string representation.
        /// </summary>
        /// <returns>The <see cref="ProductVersion"/> as string.</returns>
        public override string ToString()
        {
            if (VersionSuffix == "")
            {
                return new string(VersionNumber.ToString());
            }
            
            return VersionNumber + "-" + VersionSuffix;
        }

        /// <summary>
        /// Determines, if an instance <see cref="ProductVersion"/> equals another instance of <see cref="ProductVersion"/>
        /// </summary>
        /// <param name="obj">Another instance of <see cref="ProductVersion"/> for comparison.</param>
        /// <returns>True, if the instances ar equal on type and content. False, if they are not equal.</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || GetType() != obj.GetType()) 
            {
                return false;
            }
            
            var other = (ProductVersion) obj;
            return VersionNumber.Equals(other.VersionNumber) && VersionSuffix.Equals(other.VersionSuffix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool operator ==(
            ProductVersion obj,
            ProductVersion other)
        {
            return obj.Equals(other);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static bool operator !=(
            ProductVersion obj,
            ProductVersion other)
        {
            return !obj.Equals(other);
        }

        /// <summary>
        /// Determines the hash code of an instance of <see cref="ProductVersion"/>.
        /// </summary>
        /// <returns>The hash code of the instance.</returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(VersionNumber, VersionSuffix);
        }
        
        #endregion
    }
}