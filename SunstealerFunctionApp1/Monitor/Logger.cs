namespace Sunstealer.FunctionApp1.Monitor;

/// <summary>
/// This class is used by Adam to pull log messages via API call <see cref="Functions.Functions.Log(Microsoft.AspNetCore.Http.HttpRequest)" /> ... its probably a security risk.
/// </summary>
public class Logger
{
    /// <summary>
    /// Log messages.
    /// </summary>
    public List<string> list = new();

    /// <summary>
    /// Its is a Singleton.
    /// </summary>
    public static Logger Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    private static Logger instance { get; set; } = new();

    /// <summary>
    /// LogDebug().
    /// </summary>
    /// <param name="function"></param>
    /// <param name="message"></param>
    public void LogDebug(string function, string message)
    {
        Console.WriteLine($"[db] {function}: {message}");
        list.Add($"[db] {function}: {message}");
    }

    /// <summary>
    /// LogError().
    /// </summary>
    /// <param name="function"></param>
    /// <param name="message"></param>
    public void LogError(string function, string message)
    {
        Console.WriteLine($"[er] {function}: {message}");
        list.Add($"[er] {function}: {message}");
    }

    /// <summary>
    /// LogException().
    /// </summary>
    /// <param name="e"></param>
    /// <param name="function"></param>
    /// <param name="message"></param>
    public void LogException(Exception e, string function, string message)
    {
        Console.WriteLine($"[ex] {function}: {message} {e}");
        list.Add($"[ex] {function}: {message} {e}");
    }

    /// <summary>
    /// LogInformation().
    /// </summary>
    /// <param name="function"></param>
    /// <param name="message"></param>
    public void LogInformation(string function, string message)
    {
        Console.WriteLine($"[in] {function}: {message}");
        list.Add($"[in] {function}: {message}");
    }

    /// <summary>
    /// LogWarning().
    /// </summary>
    /// <param name="function"></param>
    /// <param name="message"></param>
    public void LogWarning(string function, string message)
    {
        Console.WriteLine($"[wn] {function}: {message}");
        list.Add($"[wn] {function}: {message}");
    }
}
