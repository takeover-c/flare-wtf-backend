using System;

namespace Flare.Base {
    public class FlareRequest {
        public string ip { get; set; }
        
        public string identity { get; set; }
        
        public string userid { get; set; }
        
        public DateTimeOffset? date { get; set; }
        
        public string method { get; set; }
        
        public string path { get; set; }
        
        public string query_string { get; set; }
        
        public int? http_version { get; set; }
    }
}