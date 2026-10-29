using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Models;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Sunstealer.FunctionApp1.Monitor;

/// <summary>
/// LoggerExtensions extends (overrides) the Microsoft Logging Framework.
/// This allows the application to log messages with source file, line number, Global and Application attributes via Open Telemetry.
/// </summary>
public static class LoggerExtensions
{
    /// <summary>
    /// The LogDebug override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogDebug(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[db] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [db] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Debug, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    /// <summary>
    /// The LogInformation override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogInformation(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[in] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [in] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Information, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    /// <summary>
    /// The LogWarning override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogWarning(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[wn] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [wn] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Warning, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    /// <summary>
    /// The LogError override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogError(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[er] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [er] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Error, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    /// <summary>
    /// The LogError (Exception) override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogError(this ILogger logger, Exception exception, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[ex] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [ex] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["exception"] = exception?.Message ?? "null",
            ["stacktrace"] = exception?.StackTrace ?? "null",
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Critical, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }

    /// <summary>
    /// The LogTrace override.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="message"></param>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    public static void LogTrace(this ILogger logger, string message, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0)
    {
        Console.WriteLine($"[tr] {filePath}:{lineNumber} {message}");
        DateTime now = DateTime.Now;
        Configuration.log.Add($"{now.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)} [tr] {message}");

        var customDimenstions = new Dictionary<string, object>
        {
            ["file"] = filePath,
            ["linenumber"] = lineNumber
        };

        using (logger.BeginScope(customDimenstions))
        {
            logger.Log(LogLevel.Trace, new EventId(0, "ajm:INFO"), customDimenstions, null, (state, exception) => message);
        }
    }
}