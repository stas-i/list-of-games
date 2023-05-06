using System.Text.Json.Serialization;

namespace Crawlers.Football.LiveSportsOdds;

public record ScoresRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; } = string.Empty;

    [JsonPropertyName("sport_title")]
    public string SportTitle { get; set; } = string.Empty;

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; } = string.Empty;

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; } = string.Empty;

    // [JsonPropertyName("scores")]
    // public string? Scores { get; set; }

    [JsonPropertyName("last_update")]
    public DateTime? LastUpdate { get; set; }
}
