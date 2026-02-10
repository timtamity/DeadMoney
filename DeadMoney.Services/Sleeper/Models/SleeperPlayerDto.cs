using System.Text.Json.Serialization;

namespace DeadMoney.Services.Sleeper.Models;

public class SleeperPlayerDto
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("team")]
    public string? Team { get; set; }

    [JsonPropertyName("position")]
    public string? Position { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}