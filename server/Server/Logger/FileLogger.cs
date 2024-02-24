using System.Runtime.CompilerServices;

namespace Server.Logger;

public class FileLogger : ILogger
{
    public static FileLogger Instance
    {
        get { return instance ??= new FileLogger(); }
    }
    
    private static FileLogger? instance;
    private static string filePath; 
    private static readonly object locker = new();

    private FileLogger() { }
    
    public static void Setup(string? pathToFolder, string? fileName)
    {
        ArgumentNullException.ThrowIfNull(pathToFolder);
        ArgumentNullException.ThrowIfNull(fileName);
        
        if (!Directory.Exists(pathToFolder))
            Directory.CreateDirectory(pathToFolder);

        lock (locker)
        {
            filePath = Path.Combine(pathToFolder, fileName);
        }
    }

    public void Warn(string? message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Log(LogLevel.Warning, message);
    }
    
    private void Log(LogLevel logLevel, string? message)
    {
        lock (locker)
        {
            using (var writer = new StreamWriter(filePath, true))
            {
                var log = $"[{logLevel}] - {DateTime.Now:G} | {message}";
                writer.WriteLine(log);
            }
        }
    }

    public void Info(string? message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Log(LogLevel.Info, message);
    }

    public void Error(string? message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Log(LogLevel.Error, message);
    }

    public void Debug(string? message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Log(LogLevel.Debug, message);
    }
}