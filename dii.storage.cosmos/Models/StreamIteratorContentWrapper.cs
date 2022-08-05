using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dii.storage.cosmos.Models
{
    /// <summary>
    /// Content wrapper for a Cosmos Stream read.
    /// </summary>
    public class StreamIteratorContentWrapper
    {
        /// <summary>
        /// The unique request id.
        /// </summary>
        [JsonPropertyName("_rid")]
        [JsonProperty("_rid")]
        public string RequestId { get; set; }

        /// <summary>
        /// The content results.
        /// </summary>
        [JsonPropertyName("Documents")]
        [JsonProperty("Documents")]
        public JsonElement[] Documents { get; set; }

        /// <summary>
        /// The total number of results.
        /// </summary>
        [JsonPropertyName("_count")]
        [JsonProperty("_count")]
        public int Count { get; set; }
    }
}