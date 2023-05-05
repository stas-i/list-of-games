using Football.Contracts.ApiData;

namespace DataWarehouse.Data.Football.Queries;

public class GetMatchByIdQuery : IDapperQuery<MatchEntity>
{
    public string Sql =>
        @"SELECT TOP (2) Code,
               StartDate as StartDateUtc,
               StartTime as StartTimeUtc,
               SportType,
               CompetitionName,
               HomeTeam,
               AwayTeam,
               UpdatedAt as LastModifiedDateUtc
FROM dbo.Matches
WHERE Code = @Code;";

    public object Params =>
        new
        {
            Code
        };

    public required string Code { get; init; }
}
