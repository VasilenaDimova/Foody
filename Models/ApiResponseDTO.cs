using System.Text.Json.Serialization;

namespace Foody.Models
{
    internal class ApiResponseDTO
    {
        //check if you need something here
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("foodId")]
        public string? FoodId { get; set; }
    }
}
