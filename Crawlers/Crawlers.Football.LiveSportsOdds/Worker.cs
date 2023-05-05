using Microsoft.Extensions.Options;

namespace Crawlers.Football.LiveSportsOdds;

public class Worker : BackgroundService
{
    private readonly MatchesSyncService _service;
    private readonly TimeSpan _pollInterval;
    private readonly ILogger<Worker> _logger;

    public Worker(MatchesSyncService service, IOptions<LiveSportsOddsOptions> options, ILogger<Worker> logger)
    {
        _service = service;
        _pollInterval = options.Value.PollInterval;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new(_pollInterval);
        do
        {
            var now = DateTimeOffset.Now;
            _logger.LogInformation("Worker running at: {Time}", now);
            var next = now.Add(_pollInterval);
            try
            {
                await _service.SyncDataAsync(stoppingToken);
                _logger.LogInformation("Worker sync completed");
            }
            catch (Exception e)
            {
                // todo: add some retry
                _logger.LogError(e, "Worker sync failed");
            }

            _logger.LogInformation("Schedule next run in: {Time}", next);
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
