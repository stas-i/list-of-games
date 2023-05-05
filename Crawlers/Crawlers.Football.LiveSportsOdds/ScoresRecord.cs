using System.Text.Json.Serialization;

namespace Crawlers.Football.LiveSportsOdds;

public record ScoresRecord
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("sport_key")]
    public string SportKey { get; set; }

    [JsonPropertyName("sport_title")]
    public string SportTitle { get; set; }

    [JsonPropertyName("commence_time")]
    public DateTime CommenceTime { get; set; }

    [JsonPropertyName("completed")]
    public bool Completed { get; set; }

    [JsonPropertyName("home_team")]
    public string HomeTeam { get; set; }

    [JsonPropertyName("away_team")]
    public string AwayTeam { get; set; }

    // [JsonPropertyName("scores")]
    // public string? Scores { get; set; }

    [JsonPropertyName("last_update")]
    public DateTime? LastUpdate { get; set; }
}
