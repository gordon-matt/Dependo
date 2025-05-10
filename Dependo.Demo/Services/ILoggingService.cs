namespace Dependo.Demo.Services;

/// <summary>
/// Service for application logging
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs a message
    /// </summary>
    /// <param name="message">Message to log</param>
    void Log(string message);
}

/// <summary>
/// Console implementation of the logging service
/// </summary>
public class ConsoleLoggingService : ILoggingService, IAutoRegisterService
{
    /// <inheritdoc/>
    public void Log(string message)
    {
        Console.WriteLine($"[LOG] {DateTime.Now}: {message}");
    }
}