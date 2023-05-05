// See https://aka.ms/new-console-template for more information

using System.Reflection;
using DbUp;

Console.WriteLine("Starting migrations");
var connectionString =
    args.FirstOrDefault()
    ?? "Server=(local)\\SqlExpress; Database=MyApp; Trusted_connection=true";

EnsureDatabase.For.SqlDatabase(connectionString);

var upgradeEngine =
    DeployChanges.To
        .SqlDatabase(connectionString)
        .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
        .LogToConsole()
        .Build();

var result = upgradeEngine.PerformUpgrade();

if (!result.Successful)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(result.Error);
    Console.ResetColor();
#if DEBUG
    Console.ReadLine();
#endif
    return -1;
}

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Success!");
Console.ResetColor();
return 0;
