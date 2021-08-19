using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Models
{
    public class Root
    {
        [JsonPropertyName("rule")]
        public Rule Rule { get; set; }
    }
    public class Rule
    {
        [JsonProperty("typefile")]
        public List<string> TypeFile { get; init; }
        
        [JsonProperty("comments")]
        public List<string> Comments { get; init; }
    }
}