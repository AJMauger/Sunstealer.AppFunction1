using System.Collections.Concurrent;

namespace Sunstealer.FunctionApp1.Middleware;

/// <summary>
/// 
/// </summary>
public static class CallContext
{
    /// <summary>
    /// 
    /// </summary>
    static ConcurrentDictionary<string, AsyncLocal<object>> state = new ConcurrentDictionary<string, AsyncLocal<object>>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="data"></param>
    public static void SetData(string name, object data) => state.GetOrAdd(name, _ => new AsyncLocal<object>()).Value = data;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static object? GetData(string name) => state.TryGetValue(name, out AsyncLocal<object>? data) ? data.Value : null;
}