using System.Text.Json.Serialization;

namespace InternetRadio.Models
{
    public class RadioStation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        // Url should carry a playable stream (prefer url_resolved from the API)
        [JsonPropertyName("url_resolved")]
        public string Url { get; set; } = string.Empty;

        // optional UI helpers
        public bool IsFavorite { get; set; } = false;
    }
}
