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
var socket = new IPEndPoint(IPAddress.Parse(ip), port);

var server = ServerTcp.GetOrCreate(socket, FileLogger.Instance);
server.Run();
logger.Debug("The program is completed.");
