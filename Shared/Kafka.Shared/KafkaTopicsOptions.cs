namespace Kafka.Shared;

public class KafkaTopicsOptions
{
    public const string KafkaTopics = "KafkaTopics";

    public KafkaTopic[] TopicsConfiguration { get; set; } = Array.Empty<KafkaTopic>();
}

public class KafkaTopic
{
    public string TopicName { get; set; } = string.Empty;
    public short ReplicationFactor { get; set; } = 2;
    public int NumPartitions { get; set; } = 10;
}
