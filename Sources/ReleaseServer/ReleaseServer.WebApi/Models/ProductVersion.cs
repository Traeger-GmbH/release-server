using System;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersion : IComparable<ProductVersion>
    {
        public Version VersionNumber { get; set; }

        public string VersionSuffix { get; set; }
        
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
        
        public override string ToString()
        {
            if (VersionSuffix == "")
            {
                return VersionNumber.ToString();
            }
            
            return VersionNumber + "-" + VersionSuffix;
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || GetType() != obj.GetType()) 
            {
                return false;
            }
            
            var other = (ProductVersion) obj;
            return VersionNumber.Equals(other.VersionNumber) && VersionSuffix.Equals(other.VersionSuffix);
        }
        
        public static bool operator ==(ProductVersion v1, ProductVersion v2)
        {
            return v1.VersionNumber == v2.VersionNumber && v1.VersionSuffix == v2.VersionSuffix;
        }

        public static bool operator !=(ProductVersion v1, ProductVersion v2)
        {
            return !(v1 == v2);
        }
    }
}