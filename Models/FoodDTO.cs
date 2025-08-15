using System.Text.Json.Serialization;

namespace Foody.Models
{
    //Adapt name to match the project
    internal class FoodDTO
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
