using System.Text.Json;
using Confluent.Kafka;
using Football.Contracts.ProviderData;
using Kafka.Shared;
using Microsoft.Extensions.Options;

namespace Crawlers.Football.RandomDataProducer;

public class Worker : BackgroundService
{
    private readonly IOptions<PublishConfig> _options;
    private readonly IKafkaProducerFactory _producerFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IOptions<PublishConfig> options,
        IKafkaProducerFactory producerFactory,
        ILogger<Worker> logger)
    {
        _options = options;
        _producerFactory = producerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = _options.Value;
        var itemsRemaining = config.NumberOfItems;
        var producer = _producerFactory.CreateProducer();
        while (!stoppingToken.IsCancellationRequested && itemsRemaining > 0)
        {
            if (itemsRemaining % 500 == 0)
            {
                _logger.LogInformation("Publishing random matches. {Count} items remaining", itemsRemaining);
            }

            var match = RandomDataGenerator.RandomFootballMatch();
            var message = new Message<string, byte[]>
            {
                Key = match.ProviderId,
                Value = JsonSerializer.SerializeToUtf8Bytes(match)
            };
            await producer.ProduceAsync(config.TopicName, message, stoppingToken);
            itemsRemaining--;
        }

        _logger.LogInformation("Publishing random matches compleed", itemsRemaining);

    }

}

public class  PublishConfig {
    public const string SectionName = "PublishConfig";
    public int NumberOfItems { get; set; } = 10000;
    public string TopicName { get; set; } = "live-sports-odds-football";
}

public static class RandomDataGenerator
{
    public static FootballMatch RandomFootballMatch()
    {
        var guid = Guid.NewGuid().ToString("N");
        var homeTeam = RandomTeam();
        string awayTeam;
        do
        {
            awayTeam = RandomTeam();
        } while (awayTeam == homeTeam);
        var generated = new FootballMatch
        {
            Provider = "Random Data Provider",
            ProviderId = guid,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            StartDate = RandomDateOnly(),
            StartTimeUtc = RandomTimeOnly(),
            CompetitionName = "Fake Data Competition"
        };

        return generated;
    }

    private static string RandomTeam()
    {
        var index = Random.Shared.Next(0, Teams.Count);
        return Teams[index];
    }
    private static DateOnly RandomDateOnly()
    {
        var month = Random.Shared.Next(6, 8);
        var day = Random.Shared.Next(15, 20);
        return new DateOnly(2023, month, day);
    }
    private static TimeOnly RandomTimeOnly()
    {
        var hour = Random.Shared.Next(17, 21);
        var minute = Random.Shared.Next(0, 4) * 15;
        return new TimeOnly(hour, minute);
    }

    private static List<string> Teams = new()
    {
        "Naples",
        "Lazio",
        "Juventus Turin",
        "Inter Milan",
        "Atalanta",
        "AC Milan",
        "Rome",
        "Fiorentina",
        "Bologne",
        "Monza",
        "Torino",
        "Udinese",
        "Sassuolo",
        "Salernitana",
        "Empoli",
        "Lecce",
        "Spezia",
        "Hellas Vérone",
        "Cremonese",
        "Sampdoria",
    };
}
