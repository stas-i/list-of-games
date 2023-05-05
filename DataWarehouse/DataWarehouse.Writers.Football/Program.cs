using DataWarehouse.Writers.Football;
using DataWarehouse.Writers.Football.Commands;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IDapperCommandExecutor, DapperCommandExecutor>();
        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();
