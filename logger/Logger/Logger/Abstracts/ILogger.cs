namespace Logger.Abstracts;

public interface ILogger
{
    public void Debug(string message);
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Fatal(string message);
    public Task DebugAsync(string message);
    public Task InfoAsync(string message);
    public Task WarnAsync(string message);
    public Task ErrorAsync(string message);
    public Task FatalAsync(string message);
}