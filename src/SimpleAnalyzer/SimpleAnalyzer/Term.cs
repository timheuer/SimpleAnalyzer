using System.Text.Json.Serialization;

namespace SimpleAnalyzer
{
    class Term
    {
        [JsonPropertyName("TermID")]
        public string Id { get; set; }

        [JsonPropertyName("Term")]
        public string Name { get; set; }

        [JsonPropertyName("Severity")]
        public string Severity { get; set; }

        [JsonPropertyName("TermClass")]
        public string Class { get; set; }

        [JsonPropertyName("Context")]
        public string Context { get; set; }

        [JsonPropertyName("ActionRecommendation")]
        public string Recommendation { get; set; }

        [JsonPropertyName("Why")]
        public string Why { get; set; }
    }
}
