using Newtonsoft.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace dii.cosmos.Models
{
    public class StreamIteratorContentWrapper
    {
        [JsonPropertyName("_rid")]
        [JsonProperty("_rid")]
        public string RequestId { get; set; }

        [JsonPropertyName("Documents")]
        [JsonProperty("Documents")]
        public JsonElement[] Documents { get; set; }

        [JsonPropertyName("_count")]
        [JsonProperty("_count")]
        public int Count { get; set; }
    }
}