using System.Text.Json.Serialization;

namespace GoldenStandard.Models;

public class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = "guest";
}

public class Review
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = "";

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("user")]
    public User? User { get; set; }
}