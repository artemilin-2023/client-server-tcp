namespace Server.Logger;

public interface ILogger
{
    public void Warn(string? message);
    public void Info(string? message);
    public void Error(string? message);
    public void Debug(string? message);
}