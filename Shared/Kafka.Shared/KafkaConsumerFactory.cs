using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Kafka.Shared;

public class KafkaConsumerFactory : IKafkaConsumerFactory
{
    private readonly IOptions<KafkaOptions> _options;

    public KafkaConsumerFactory(IOptions<KafkaOptions> options)
    {
        _options = options;
    }

    public IConsumer<string, byte[]> CreateConsumer(ConsumerSettings settings)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _options.Value.BootstrapServers,
            GroupId = settings.GroupId,
            EnableAutoOffsetStore = false,
            EnableAutoCommit = false,
            StatisticsIntervalMs = 5000,
            SessionTimeoutMs = 6000,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // A good introduction to the CooperativeSticky assignor and incremental rebalancing:
            // https://www.confluent.io/blog/cooperative-rebalancing-in-kafka-streams-consumer-ksqldb/
            PartitionAssignmentStrategy = PartitionAssignmentStrategy.CooperativeSticky
        };

        var consumer = new ConsumerBuilder<string, byte[]>(config)
            // Note: All handlers are called on the main .Consume thread.
            .SetErrorHandler((_, e) => Console.WriteLine($"Error: {e.Reason}"))
            //.SetStatisticsHandler((_, json) => Console.WriteLine($"Statistics: {json}"))
            .SetPartitionsAssignedHandler((c, partitions) =>
            {
                // Since a cooperative assignor (CooperativeSticky) has been configured, the
                // partition assignment is incremental (adds partitions to any existing assignment).
                Console.WriteLine(
                    "Partitions incrementally assigned: [" +
                    string.Join(',', partitions.Select(p => p.Partition.Value)) +
                    "], all: [" +
                    string.Join(',', c.Assignment.Concat(partitions).Select(p => p.Partition.Value)) +
                    "]");

                // Possibly manually specify start offsets by returning a list of topic/partition/offsets
                // to assign to, e.g.:
                // return partitions.Select(tp => new TopicPartitionOffset(tp, externalOffsets[tp]));
            })
            .SetPartitionsRevokedHandler((c, partitions) =>
            {
                // Since a cooperative assignor (CooperativeSticky) has been configured, the revoked
                // assignment is incremental (may remove only some partitions of the current assignment).
                var remaining = c.Assignment.Where(atp => partitions.All(rtp => rtp.TopicPartition != atp));
                Console.WriteLine(
                    "Partitions incrementally revoked: [" +
                    string.Join(',', partitions.Select(p => p.Partition.Value)) +
                    "], remaining: [" +
                    string.Join(',', remaining.Select(p => p.Partition.Value)) +
                    "]");
            })
            .SetPartitionsLostHandler((c, partitions) =>
            {
                // The lost partitions handler is called when the consumer detects that it has lost ownership
                // of its assignment (fallen out of the group).
                Console.WriteLine($"Partitions were lost: [{string.Join(", ", partitions)}]");
            })
            .Build();

        return consumer;
    }
}

public interface IKafkaConsumerFactory
{
    IConsumer<string, byte[]> CreateConsumer(ConsumerSettings settings);
}

public class ConsumerSettings
{
    public string TopicName { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
}
