using Logger.Abstracts;

namespace Logger
{
    internal class ConsoleLogger : ILogger
    {
        public static LogLevel? Level { get; private set; } = LogLevel.Debug;

        private static readonly RWLocker locker = new();

        internal ConsoleLogger(LogLevel? logLevel)
        {
            Level = logLevel;
        }

        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        private void Log(LogLevel level, string message)
        {
            if (Level <= level)
            {
                using (locker.StartWrite())
                {
                    Console.ForegroundColor = level.GetColor();
                    Console.Write($"[{level}]");
                    Console.ResetColor();

                    var log = $" - {DateTime.Now:G} | {message}";
                    Console.WriteLine(log);
                }
            }
        }

        public async Task DebugAsync(string message)
        {
            await LogAsync(LogLevel.Debug, message);
        }

        private async Task LogAsync(LogLevel level, string message)
        {
            if (Level <= level)
            {
                using (locker.StartWrite())
                {
                    Console.ForegroundColor = level.GetColor();
                    await Console.Out.WriteAsync($"[{level}]");
                    Console.ResetColor();

                    var log = $" - {DateTime.Now:G} | {message}";
                    await Console.Out.WriteLineAsync(log);
                }
            }
        }

        public void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        public async Task ErrorAsync(string message)
        {
            await LogAsync(LogLevel.Error, message);
        }

        public void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        public async Task FatalAsync(string message)
        {
            await LogAsync(LogLevel.Fatal, message);
        }

        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        public async Task InfoAsync(string message)
        {
            await LogAsync(LogLevel.Info, message);
        }

        public void Warn(string message)
        {
            Log(LogLevel.Warning, message);
        }

        public async Task WarnAsync(string message)
        {
            await LogAsync(LogLevel.Warning, message);
        }
    }
}
