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

    public IProducer<string, string> CreateProducer()
    {
        var config = new ProducerConfig { BootstrapServers = _options.Value.BootstrapServers };
        return new ProducerBuilder<string, string>(config).Build();
    }
}

public interface IKafkaProducerFactory
{
    IProducer<string, string> CreateProducer();
}
