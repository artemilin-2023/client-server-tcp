using System.Net;
using Microsoft.Extensions.Configuration;
using Server;
using Server.Logger;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json");
var configuration = builder.Build();

FileLogger.Setup(configuration["Logging:Folder"], configuration["Logging:File"]);
var logger = FileLogger.Instance;

logger.Debug("Run program.");

var ip = configuration["Server:IPv4"]!;
var port = configuration.GetValue<int>("Server:Port");

try
{
    var server = ServerTcp.GetOrCreate(ip, port, logger);
    server.Init();
    server.Run();
    logger.Debug("The program is completed.");
}
catch (Exception ex)
{
    logger.Error(ex.Message);
}
finally
{
    // TODO: реализовать остановку сервера
}