namespace Logger.Abstracts;

public enum LogLevel
{
    Debug, Info, Warning, Error, Fatal
}

internal static class EnumExtension
{
    internal static ConsoleColor GetColor(this LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => ConsoleColor.White,
            LogLevel.Info => ConsoleColor.Green,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Fatal => ConsoleColor.DarkRed
        };
    }
}