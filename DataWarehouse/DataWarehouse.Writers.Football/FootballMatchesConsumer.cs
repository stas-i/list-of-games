using System.Text.Json;
using Confluent.Kafka;
using DataWarehouse.Data.Football;
using DataWarehouse.Data.Football.Commands;
using Football.Contracts.Events;
using Football.Contracts.ProviderData;
using Kafka.Shared;
using Microsoft.Extensions.Options;

namespace DataWarehouse.Writers.Football;

public class FootballMatchesConsumer : BackgroundService
{
    private readonly IConsumer<string, byte[]> _consumer;
    private readonly IKafkaProducerFactory _kafkaProducerFactory;
    private readonly IDapperExecutor _executor;
    private readonly IOptions<FootballMatchesConsumerSettingsOptions> _consumerOptions;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FootballMatchesConsumer> _logger;

    public FootballMatchesConsumer(
        IKafkaConsumerFactory consumerFactory,
        IKafkaProducerFactory kafkaProducerFactory,
        IDapperExecutor executor,
        IOptions<FootballMatchesConsumerSettingsOptions> consumerOptions,
        IConfiguration configuration,
        ILogger<FootballMatchesConsumer> logger)
    {
        _consumer = consumerFactory.CreateConsumer(consumerOptions.Value);
        _kafkaProducerFactory = kafkaProducerFactory;
        _executor = executor;
        _consumerOptions = consumerOptions;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await StartConsumerLoop(stoppingToken);
    }

    private async Task StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_consumerOptions.Value.TopicName);
        var sinksEventsProducer = _kafkaProducerFactory.CreateProducer();
        var sinkEventsTopic = _configuration["Sinks:TopicName"] ?? "data-sinks-topic";

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                var matchCode = await SaveData(consumeResult, cancellationToken);
                await FanOutEvent(sinkEventsTopic, sinksEventsProducer, matchCode, cancellationToken);
                _logger.LogDebug("Received message at {Offset}: {Key}", consumeResult.TopicPartitionOffset, consumeResult.Message.Key);

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
                _logger.LogError(e, "Consume error");

                if (e.Error.IsFatal)
                {
                    // https://github.com/edenhill/librdkafka/blob/master/INTRODUCTION.md#fatal-consumer-errors
                    break;

                }
            }
            catch (Exception e)
            {
                _logger.LogError(e,"Unexpected error");
            }
        }
    }

    private async Task<string?> SaveData(ConsumeResult<string, byte[]> message, CancellationToken cancellationToken)
    {
        var messageValue = JsonSerializer.Deserialize<FootballMatch>(message.Message.Value);
        if (messageValue is null)
        {
            return null;
        }
        // TODO: Some smart code optimizations maybe will appear here :)
        // 1 - Id Mapping cache. Create mem cache with mapping from provider id to db key
        // 2 - Do get items by filer from db before saving - avoid unnecessary locks if it is exists

        var command = new SaveMatchCommand
        {
            Code = GenerateCode(messageValue),
            StartDate = messageValue.StartDate,
            StartTime = messageValue.StartTimeUtc,
            HomeTeam = messageValue.HomeTeam,
            AwayTeam = messageValue.AwayTeam,
            CompetitionName = messageValue.CompetitionName,
            SportType = "Football",
        };

        var output = await _executor.QueryAsync(command, cancellationToken);
        if (output.Count != 1)
        {
            // BUG: If we have match 1 with teams X and Y  at 19:00 and same teams play at 22:00 and then
            // third match comes with start time 20:30 (less than two hours to both matches). Query updates both matches :(
            _logger.LogError("Three or more matches for same teams {@Output} - {Input}", output, command.Code);
        }

        var code = output.FirstOrDefault()?.UpdatedItemCode ?? command.Code;
        return code;
    }

    private async Task FanOutEvent(
        string sinkEventsTopic,
        IProducer<string, byte[]> producer,
        string? matchCode,
        CancellationToken cancellationToken)
    {
        if (matchCode is null)
        {
            return;
        }

        var updateEvent = new MatchUpdatedEvent()
        {
            Code = matchCode
        };
        var message = new Message<string, byte[]>
        {
            Key = matchCode,
            Value = JsonSerializer.SerializeToUtf8Bytes(updateEvent)
        };

        // Note: Awaiting the asynchronous produce request below prevents flow of execution
        // from proceeding until the acknowledgement from the broker is received (at the
        // expense of low throughput).
        var deliveryReport = await producer.ProduceAsync(sinkEventsTopic, message, cancellationToken);
    }

    private string GenerateCode(FootballMatch match)
    {
        // TODO: Some idempotent method to generate unique id should be there
        return $"FTB-MAT-{match.StartDate:O}-{match.HomeTeam[..3]}-{match.AwayTeam[..3]}-{match.StartTimeUtc:t}".PadRight(32,'-');
    }

    public override void Dispose()
    {
        _consumer.Close(); // Commit offsets and leave the group cleanly.
        _consumer.Dispose();

        base.Dispose();
    }
}

public class FootballMatchesConsumerSettingsOptions : ConsumerSettings
{
    public const string FootballMatchesConsumerSettings = "ConsumersConfiguration:FootballMatches";
}
