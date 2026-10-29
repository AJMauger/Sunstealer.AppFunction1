using Microsoft.Extensions.Logging;
using Sunstealer.FunctionApp1.Services;

namespace Sunstealer.FunctionApp1.Monitor;

/// <summary>
/// The StackFrame class assigns Metric attrubutes depending on application <see cref="ApplicationService._logLevel" />.
/// If <see cref="ApplicationService._logLevel" /> is <see cref="LogLevel.Trace" /> or <see cref="LogLevel.Debug" /> the file path, line number, calling method name and return Type are assigned.  
/// If <see cref="ApplicationService._logLevel" /> is <see cref="LogLevel.Information" /> the calling method name and return Type are assigned.  
/// </summary>
public class StackFrame
{
    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="lineNumber"></param>
    /// <returns><cref name="Dictionary" /></returns>
    public static Dictionary<string, string> GetStackFrame(string filePath, int lineNumber)
    {
        var properties = new Dictionary<string, string>();

        System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(2);
        var method = sf.GetMethod();

        switch (ApplicationService._logLevel)
        {
            case LogLevel.Trace:
                goto case LogLevel.Debug;
            case LogLevel.Debug:
                properties.Add("filePath", filePath);
                properties.Add("lineNumber", lineNumber.ToString());
                goto case LogLevel.Information;
            case LogLevel.Information:
                properties.Add("method", $"{method?.DeclaringType?.FullName}.{method?.Name}");
                goto case LogLevel.Warning;
            case LogLevel.Warning:
                goto case LogLevel.Error;
            case LogLevel.Error:
                goto case LogLevel.Critical;
            case LogLevel.Critical:
                break;
        }
        return properties;
    }
}
