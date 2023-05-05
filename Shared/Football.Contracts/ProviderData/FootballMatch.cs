namespace Football.Contracts.ProviderData;

public class FootballMatch
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public TimeOnly StartTimeUtc { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public string CompetitionName { get; set; } = string.Empty;
}

