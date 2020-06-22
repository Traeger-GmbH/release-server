using System.Collections.Generic;

namespace ReleaseServer.WebApi.Models
{
    public class ChangeSet
    {
        public List<string> Platforms { get; set; }
    
        public List<string> Added { get; set; }
    
        public List<string> Fixed { get; set; }
    
        public List<string> Breaking { get; set; }
    
        public List<string> Deprecated { get; set; }
    }
}