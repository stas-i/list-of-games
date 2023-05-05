using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kafka.Shared;

public class TopicCreationBackgroundService : BackgroundService
{
    private readonly ILogger<TopicCreationBackgroundService> _logger;
    private readonly IOptions<KafkaOptions> _kafkaOptions;
    private readonly IOptions<KafkaTopicsOptions> _topicsOptions;

    public TopicCreationBackgroundService(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<KafkaTopicsOptions> topicsOptions,
        IConfiguration configuration,
        ILogger<TopicCreationBackgroundService> logger)
    {
        _kafkaOptions = kafkaOptions;
        _topicsOptions = topicsOptions;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var topics = _topicsOptions.Value.TopicsConfiguration
            .Select(x => new TopicSpecification
            {
                Name = x.TopicName, ReplicationFactor = x.ReplicationFactor, NumPartitions = x.NumPartitions
            })
            .ToArray();
        if (!topics.Any())
        {
            throw new Exception("No topics found in config. Remove AddTopics registration or add topics");
        }

        var adminClientConfig = new AdminClientConfig { BootstrapServers = _kafkaOptions.Value.BootstrapServers };

        using var adminClient = new AdminClientBuilder(adminClientConfig).Build();
        try
        {
            await adminClient.CreateTopicsAsync(topics);
            _logger.LogInformation("{Count} topics created", topics.Length);
        }
        catch (CreateTopicsException e)
        {
            if (e.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
            {
                _logger.LogInformation("Topic(s) already exists");
            }
            else
            {
                _logger.LogError(e, "Failed to create topic");
                throw;
            }

        }
    }
}
