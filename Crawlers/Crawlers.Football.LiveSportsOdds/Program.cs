using Crawlers.Football.LiveSportsOdds;
using Kafka.Shared;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((services) =>
    {
        services.AddOptions<LiveSportsOddsOptions>().BindConfiguration(LiveSportsOddsOptions.LiveSportsOdds);
        services.RegisterKafka();
        services.AddTopics();
        services.AddHttpClient();
        services.AddSingleton<MatchesSyncService>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
