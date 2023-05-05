using Football.Contracts;

namespace DataWarehouse.Writers.Football.Commands;

public class SaveMatchCommand : IDapperCommand
{
    public string Sql =>
        @"MERGE dbo.Matches WITH (SERIALIZABLE) AS Existing
USING (VALUES (@Code,
               @StartDate,
               @StartTime,
               @SportType,
               @CompetitionName,
               @HomeTeam,
               @AwayTeam)) AS Incoming (Code, StartDate, StartTime, SportType, CompetitionName, HomeTeam, AwayTeam)
ON Existing.CompetitionName = Incoming.CompetitionName AND
   Existing.SportType = Incoming.SportType AND
   Existing.StartDate = Incoming.StartDate AND
   ((Existing.HomeTeam = Incoming.HomeTeam AND Existing.AwayTeam = Incoming.AwayTeam) OR
    (Existing.HomeTeam = Incoming.AwayTeam AND Existing.AwayTeam = Incoming.HomeTeam)) AND
   ABS(DATEDIFF(minute, Existing.StartTime, Incoming.StartTime)) < @TimeDiffInMinutes
WHEN MATCHED THEN
    UPDATE
    SET Existing.UpdatedAt = GETUTCDATE(),
        Existing.StartTime = Incoming.StartTime
WHEN NOT MATCHED THEN
    INSERT (Code,
            StartDate,
            StartTime,
            SportType,
            CompetitionName,
            HomeTeam,
            AwayTeam,
            CreatedAt,
            UpdatedAt)
    VALUES (Incoming.Code,
            Incoming.StartDate,
            Incoming.StartTime, SportType,
            Incoming.CompetitionName,
            Incoming.HomeTeam,
            Incoming.AwayTeam,
            GETUTCDATE(),
            GETUTCDATE());";

    public object Params =>
        new
        {
            Code,
            StartDate = StartDate.ToDateTime(TimeOnly.MinValue),
            StartTime = StartTime.ToTimeSpan(),
            SportType,
            CompetitionName,
            HomeTeam,
            AwayTeam,
            TimeDiffInMinutes
        };

    public required string Code { get; init; }
    public DateOnly StartDate { get; init; }
    public TimeOnly StartTime { get; init; }
    public string SportType { get; init; } = "Football";
    public required string CompetitionName { get; init; }
    public required  string HomeTeam { get; init; }
    public required  string AwayTeam { get; init; }
    public int TimeDiffInMinutes { get; init; } = 120;
}
