using Microsoft.Extensions.DependencyInjection;

namespace Kafka.Shared;

public static class ServiceCollectionsExtensions
{
    public static void RegisterKafka(this IServiceCollection services)
    {
        services.AddOptions<KafkaOptions>().BindConfiguration(KafkaOptions.Kafka);
        services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
        services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
    }

    public static void AddTopics(this IServiceCollection services)
    {
        services.AddOptions<KafkaTopicsOptions>().BindConfiguration(KafkaTopicsOptions.KafkaTopics);
        services.AddHostedService<TopicCreationBackgroundService>();
    }
}
