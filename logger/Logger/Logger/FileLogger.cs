using Logger.Abstracts;

namespace Logger;

internal class FileLogger : ILogger
{
    public static LogLevel? Level { get; private set; } = LogLevel.Debug;
    
    private static string filePath; 
    private static readonly object locker = new();

    internal FileLogger(string path, LogLevel? logLevel) 
    {
        filePath = path;
        Level = logLevel;
    }

    public void Warn(string message)
    {
        Log(LogLevel.Warning, message);
    }
    
    private void Log(LogLevel level, string message)
    {
        if (Level <= level)
        {
            lock (locker)
            {
                using (var writer = new StreamWriter(filePath, true))
                {
                    var log = $"[{level}] - {DateTime.Now:G} | {message}";
                    writer.WriteLine(log);
                }
            }
        }
    }

    public void Info(string message)
    {
        Log(LogLevel.Info, message);
    }

    public void Error(string message)
    {
        Log(LogLevel.Error, message);
    }

    public void Debug(string message)
    {
        Log(LogLevel.Debug, message);
    }

    public void Fatal(string message)
    {
        Log(LogLevel.Fatal, message);
    }

    public async Task DebugAsync(string message)
    {
        await LogAsync(LogLevel.Debug, message);
    }

    private async Task LogAsync(LogLevel level, string message)
    {
        if (Level <= level)
        {
            using (var writer = new StreamWriter(filePath, true))
            {
                var log = $"[{level}] - {DateTime.Now:G} | {message}";
                await writer.WriteLineAsync(message);
            }
        }
    }

    public async Task InfoAsync(string message)
    {
        await LogAsync(LogLevel.Info, message);
    }

    public async Task WarnAsync(string message)
    {
        await LogAsync(LogLevel.Warning, message);
    }

    public async Task ErrorAsync(string message)
    {
        await LogAsync(LogLevel.Error, message);
    }

    public async Task FatalAsync(string message)
    {
        await LogAsync(LogLevel.Fatal, message);
    }
}