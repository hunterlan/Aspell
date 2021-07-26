using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Models
{
    public class Rule
    {
        [JsonPropertyName("typefile")]
        public List<string> TypeFile { get; init; }
        
        [JsonPropertyName("typefile")]
        public List<string> Comments { get; init; }
    }
}