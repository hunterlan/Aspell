using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class Root
    {
        [JsonProperty("rule")]
        public List<Rule> Rule { get; set; }
    }
    public class Rule
    {
        [JsonProperty("typefile")]
        public List<string> TypeFile { get; init; }
        
        [JsonProperty("comments")]
        public List<string> Comments { get; init; }
    }
}