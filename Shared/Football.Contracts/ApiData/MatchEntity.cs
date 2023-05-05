namespace Football.Contracts.ApiData;

public class MatchEntity
{
    public string Code { get; init; } = string.Empty;
    public DateTime StartDateUtc { get; init; }
    public TimeSpan StartTimeUtc { get; init; }
    public string SportType { get; init; } = string.Empty;
    public string CompetitionName { get; init; } = string.Empty;
    public string HomeTeam { get; init; } = string.Empty;
    public string AwayTeam { get; init; } = string.Empty;
    public DateTime LastModifiedDateUtc { get; init; }
}
