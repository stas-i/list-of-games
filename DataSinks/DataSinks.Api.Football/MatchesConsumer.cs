using System.Text.Json;
using Confluent.Kafka;
using Football.Contracts.ApiData;
using Football.Contracts.Events;
using Kafka.Shared;
using Microsoft.Extensions.Options;

namespace DataSinks.Api.Football;

public class MatchesConsumer : BackgroundService
{
    private readonly IConsumer<string, byte[]> _consumer;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _warehouseUri;
    private readonly DataService _dataService;
    private readonly IOptions<MatchesConsumerOptions> _consumerOptions;

    public MatchesConsumer(
        IKafkaConsumerFactory consumerFactory,
        DataService dataService,
        IOptions<MatchesConsumerOptions> consumerOptions,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory)
    {
        _consumer = consumerFactory.CreateConsumer(consumerOptions.Value);
        _dataService = dataService;
        _consumerOptions = consumerOptions;
        _httpClientFactory = httpClientFactory;
        _warehouseUri = configuration["ApiHosts:WarehouseApi"] ?? "https://localhost:7263";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartConsumerLoop(stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_consumerOptions.Value.TopicName);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                await SaveData(consumeResult, cancellationToken);

                Console.WriteLine(
                    $"Received message at {consumeResult.TopicPartitionOffset}: {consumeResult.Message.Value}");

                // Store the offset associated with consumeResult to a local cache. Stored offsets are committed to Kafka by a background thread every AutoCommitIntervalMs.
                // The offset stored is actually the offset of the consumeResult + 1 since by convention, committed offsets specify the next message to consume.
                // If EnableAutoOffsetStore had been set to the default value true, the .NET client would automatically store offsets immediately prior to delivering messages to the application.
                // Explicitly storing offsets after processing gives at-least once semantics, the default behavior does not.
                _consumer.StoreOffset(consumeResult);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException e)
            {
                // Consumer errors should generally be ignored (or logged) unless fatal.
                Console.WriteLine($"Consume error: {e.Error.Reason}");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e}");
                break;
            }
        }
    }

    private async Task SaveData(ConsumeResult<string, byte[]> message, CancellationToken cancellationToken)
    {
        var messageValue = JsonSerializer.Deserialize<MatchUpdatedEvent>(message.Message.Value);
        if (messageValue is null)
        {
            return;
        }

        var httpClient = _httpClientFactory.CreateClient();
        var matchEntity = await httpClient.GetFromJsonAsync<MatchEntity>($"{_warehouseUri}/matches/{messageValue.Code}",
            cancellationToken);
        if (matchEntity == null)
        {
            throw new ArgumentException($"Match with code {messageValue.Code} was not found");
        }

        var modelToSave = new Match
        {
            Code = matchEntity.Code,
            StartDateUtc = matchEntity.StartDateUtc,
            StartTimeUtc = matchEntity.StartTimeUtc,
            HomeTeam = matchEntity.HomeTeam,
            AwayTeam = matchEntity.AwayTeam,
            LastModifiedDateUtc = matchEntity.LastModifiedDateUtc,
            CompetitionName = matchEntity.CompetitionName,
            SportType = matchEntity.SportType,

        };

        await _dataService.SaveAsync(modelToSave, cancellationToken);
    }

    public override void Dispose()
    {
        _consumer.Close(); // Commit offsets and leave the group cleanly.
        _consumer.Dispose();

        base.Dispose();
    }
}

public class MatchesConsumerOptions : ConsumerSettings
{
    public const string SectionName = "ConsumersConfiguration:FootballMatches";
}
