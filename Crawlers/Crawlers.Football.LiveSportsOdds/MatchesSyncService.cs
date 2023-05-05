using System.Text.Json;
using Confluent.Kafka;
using Football.Contracts;
using Football.Contracts.ProviderData;
using Kafka.Shared;
using Microsoft.Extensions.Options;

namespace Crawlers.Football.LiveSportsOdds;

public class MatchesSyncService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IKafkaProducerFactory _producerFactory;
    private readonly IOptions<LiveSportsOddsOptions> _options;
    private readonly ILogger<MatchesSyncService> _logger;
    private const string Provider = "LiveSportsOdds";

    public MatchesSyncService(
        IHttpClientFactory httpClientFactory,
        IKafkaProducerFactory producerFactory,
        IOptions<LiveSportsOddsOptions> options,
        ILogger<MatchesSyncService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _producerFactory = producerFactory;
        _options = options;
        _logger = logger;
    }

    public async Task SyncDataAsync(CancellationToken cancellationToken)
    {
        try
        {
            var config = _options.Value;
            var response = await SendRequest(cancellationToken, config);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var producer = _producerFactory.CreateProducer();
            var records = JsonSerializer.DeserializeAsyncEnumerable<ScoresRecord>(stream, cancellationToken: cancellationToken);
            await foreach (var record in records.WithCancellation(cancellationToken))
            {
                await PublishData(record, producer, config, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to sync data");
            throw;
        }
    }

    private Task<HttpResponseMessage> SendRequest(CancellationToken cancellationToken, LiveSportsOddsOptions config)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{config.Host}{config.ChinaSuperLeagueScores}"),
            Headers =
            {
                { "X-RapidAPI-Key", config.ApiKey },
                { "X-RapidAPI-Host", config.ApiHost },
            },
        };

        return client.SendAsync(request, cancellationToken);
    }

    private static async Task PublishData(
        ScoresRecord? record,
        IProducer<string, byte[]> producer,
        LiveSportsOddsOptions config,
    CancellationToken cancellationToken
        )
    {
        if (record is null)
        {
            return;
        }

        var mapped = new FootballMatch
        {
            Provider = Provider,
            ProviderId = record.Id,
            HomeTeam = record.HomeTeam,
            AwayTeam = record.AwayTeam,
            StartDate = DateOnly.FromDateTime(record.CommenceTime),
            StartTimeUtc = TimeOnly.FromDateTime(record.CommenceTime),
            CompetitionName = record.SportTitle
        };

        var message = new Message<string, byte[]>
        {
            Key = mapped.ProviderId,
            Value = JsonSerializer.SerializeToUtf8Bytes(mapped)
        };

        // Note: Awaiting the asynchronous produce request below prevents flow of execution
        // from proceeding until the acknowledgement from the broker is received (at the
        // expense of low throughput).
        var deliveryReport = await producer.ProduceAsync(config.TopicName, message, cancellationToken);
    }
}
