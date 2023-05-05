using DataWarehouse.Writers.Football.Commands;

namespace DataWarehouse.Writers.Football;

public class Worker : BackgroundService
{
    private readonly IDapperCommandExecutor _executor;
    private readonly ILogger<Worker> _logger;

    public Worker(IDapperCommandExecutor executor, ILogger<Worker> logger)
    {
        _executor = executor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.Now;
            var command = new SaveMatchCommand
            {
                Code = Guid.NewGuid().ToString("N"),
                StartDate = DateOnly.FromDateTime(now.UtcDateTime),
                StartTime = TimeOnly.FromDateTime(now.UtcDateTime),
                HomeTeam = "Torino",
                AwayTeam = "Milano",
                CompetitionName = "Serie A"
            };

            var result = await _executor.Execute(command, stoppingToken);
            _logger.LogInformation("Worker running at: {Time}. Result is {Result}", now, result);
            await Task.Delay(10000, stoppingToken);
        }
    }
}
