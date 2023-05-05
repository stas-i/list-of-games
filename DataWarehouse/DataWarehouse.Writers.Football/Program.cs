using DataWarehouse.Data.Football;
using DataWarehouse.Data.Football.Commands;
using DataWarehouse.Writers.Football;
using Kafka.Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddOptions<FootballMatchesConsumerSettingsOptions>()
            .BindConfiguration(FootballMatchesConsumerSettingsOptions.FootballMatchesConsumerSettings);
        services.AddSingleton<IDapperExecutor, DapperExecutor>();
        services.RegisterKafka();
        services.AddTopics();
        services.AddHostedService<FootballMatchesConsumer>();
    })
    .Build();

host.Run();
