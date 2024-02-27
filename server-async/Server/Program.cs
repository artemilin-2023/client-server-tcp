﻿using Logger;
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



logger.Debug("Run program.");

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
    logger.Fatal(ex.Message);
}
finally
{
    server.Stop();
    logger.Info("Сервер был остановлен.");
}
logger.Debug("The program is completed.");

#if DEBUG
Console.ReadLine();
#endif