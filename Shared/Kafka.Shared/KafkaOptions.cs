namespace Kafka.Shared;

public class KafkaOptions
{
    public const string Kafka = "Kafka";

    public string BootstrapServers { get; set; } = string.Empty;
}
