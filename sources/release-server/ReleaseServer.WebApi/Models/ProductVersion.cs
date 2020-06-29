using System;

namespace ReleaseServer.WebApi.Models
{
    public class ProductVersion : IComparable<ProductVersion>
    {
        public Version VersionNumber { get; set; }

        public string VersionSuffix { get; set; }

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
                return new string(VersionNumber.ToString());
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
        
        protected bool Equals(ProductVersion other)
        {
            return Equals(VersionNumber, other.VersionNumber) && VersionSuffix == other.VersionSuffix;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VersionNumber, VersionSuffix);
        }
        
        
    }
}