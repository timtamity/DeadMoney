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
    
    [JsonPropertyName("active")]
    public bool Active { get; set; }
    [JsonPropertyName("college")]
    public string? College { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("height")]
    public string? Height { get; set; }

    [JsonPropertyName("weight")]
    public string? Weight { get; set; }

    [JsonPropertyName("years_exp")]
    public object? YearsExp { get; set; } // Sleeper sometimes sends "R" or a number

    [JsonPropertyName("number")]
    public int? Number { get; set; }
}