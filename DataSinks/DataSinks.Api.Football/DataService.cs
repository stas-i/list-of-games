using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DataSinks.Api.Football;

public class DataService
{
    private readonly IMongoCollection<Match> _matchesCollection;

    public DataService(IOptions<MongoOptions> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(options.Value.DatabaseName);

        _matchesCollection = mongoDatabase.GetCollection<Match>(nameof(Match));
    }

    public async Task<List<Match>> GetAsync(MatchesFilter filter, CancellationToken cancellationToken)
    {
        IFindFluent<Match, Match>? result;
        if (filter.Team != null)
        {
            result = _matchesCollection.Find(m => m.AwayTeam.Contains(filter.Team) || m.HomeTeam.Contains(filter.Team));
        }
        else
        {
            result = _matchesCollection.Find(_ => true);
        }

        return await result.SortByDescending(x => x.StartDateUtc).ToListAsync(cancellationToken);
    }


    public async Task<Match?> GetAsync(string code, CancellationToken cancellationToken) =>
        await _matchesCollection.Find(x => x.Code == code).FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public async Task SaveAsync(Match match, CancellationToken cancellationToken)
    {
        await _matchesCollection.ReplaceOneAsync(x => x.Code == match.Code, match, new ReplaceOptions
        {
            IsUpsert = true
        }, cancellationToken: cancellationToken);
    }

}

public class Match
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Code { get; set; } = string.Empty;

    public DateTime StartDateUtc { get; init; }
    public TimeSpan StartTimeUtc { get; init; }
    public string SportType { get; init; } = string.Empty;
    public string CompetitionName { get; init; } = string.Empty;
    public string HomeTeam { get; init; } = string.Empty;
    public string AwayTeam { get; init; } = string.Empty;
    public DateTime LastModifiedDateUtc { get; init; }
}

public class MatchesFilter
{
    public string? Team { get; set; }
}
