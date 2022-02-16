using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    /// <summary>
    /// Class for deserialize JSON file.
    /// </summary>
    public class Root
    {
        [JsonProperty("rule")]
        public List<Rule> Rule { get; set; }
    }
    
    /// <summary>
    /// Class, which show construction of file rules.json
    /// </summary>
    public class Rule
    {
        /// <summary>
        /// Contains list of extensions of files.
        /// </summary>
        [JsonProperty("typefile")]
        public List<string> TypeFile { get; init; }
        
        /// <summary>
        /// Type comment for every type file.
        /// </summary>
        [JsonProperty("comments")]
        public List<string> Comments { get; init; }
    }
}