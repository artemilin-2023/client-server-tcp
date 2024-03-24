using Logger;
using Logger.Abstracts;
using Microsoft.Extensions.Configuration;
using Server;

# if DEBUG
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.development.json")
    .Build();

var logger = new LoggerFabric()
    .SetJsonConfiguration("appsettings.development.json")
    .Build();
# endif

# if (!DEBUG)
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json").Build();

var logger = new LoggerFabric()
    .SetJsonConfiguration("appsettings.json")
    .Build();
# endif

logger = new LoggerFabric().SetLogLevel(LogLevel.Debug).Build();

await logger.DebugAsync("Run program.");

var ip = configuration["Server:IPv4"]!;
var port = configuration.GetValue<int>("Server:Port");
var server = ServerTcp.GetOrCreate(ip, port, logger);

try
{
    await server.Init();
    await server.RunAsync();
}
catch (Exception ex)
{
    await logger.FatalAsync(ex.Message);
}
finally
{
    server.Stop();
    await logger.InfoAsync("Сервер был остановлен.");
}
await logger.DebugAsync("The program is completed.");