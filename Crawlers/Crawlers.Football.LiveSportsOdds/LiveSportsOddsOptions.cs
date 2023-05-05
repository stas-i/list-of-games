namespace Crawlers.Football.LiveSportsOdds;

public class LiveSportsOddsOptions
{
    public const string LiveSportsOdds = "LiveSportsOdds";

    public string ApiKey { get; set; } = string.Empty;
    public string ApiHost { get; set; } = string.Empty;
    public string Host { get; set; } = string.Empty;
    public string ChinaSuperLeagueScores { get; set; } = string.Empty;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromHours(12);
    public string TopicName { get; set; } = string.Empty;
}
