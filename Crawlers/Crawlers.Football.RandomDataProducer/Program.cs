using Crawlers.Football.RandomDataProducer;
using Kafka.Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<PublishConfig>().BindConfiguration(PublishConfig.SectionName);
        services.RegisterKafka();
        services.AddTopics();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
