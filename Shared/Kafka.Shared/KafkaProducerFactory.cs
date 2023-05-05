using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Kafka.Shared;

public class KafkaProducerFactory : IKafkaProducerFactory
{
    private readonly IOptions<KafkaOptions> _options;

    public KafkaProducerFactory(IOptions<KafkaOptions> options)
    {
        _options = options;
    }

    public IProducer<string, byte[]> CreateProducer()
    {
        var config = new ProducerConfig { BootstrapServers = _options.Value.BootstrapServers };
        return new ProducerBuilder<string, byte[]>(config).Build();
    }
}

public interface IKafkaProducerFactory
{
    IProducer<string, byte[]> CreateProducer();
}
