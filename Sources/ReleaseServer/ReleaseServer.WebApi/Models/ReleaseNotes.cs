using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace ReleaseServer.WebApi.Models
{
    public class ReleaseNotes
    {
        public Dictionary<CultureInfo, List<ChangeSet>> Changes { get; set;}
    }
    
    public class ChangeSet
    {
        public List<string> Platforms { get; set; }
        
        public List<string> Added { get; set; }
        
        public List<string> Fixed { get; set; }
        
        public List<string> Breaking { get; set; }
        
        public List<string> Deprecated { get; set; }
    }
}