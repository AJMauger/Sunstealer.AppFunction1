namespace Sunstealer.FunctionApp1.Middleware;

/// <summary>
/// 
/// </summary>
public static class Flow
{
    /// <summary>
    /// 
    /// </summary>
    public const string FlowIdName = "flowId";
    /// <summary>
    /// 
    /// </summary>
    public const string SpanIdName = "spanId";
    /// <summary>
    /// 
    /// </summary>
    public const string ParentIdName = "parentId";

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string CreateFlowId()
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static string CreateSpanId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flowId"></param>
    /// <param name="parentId"></param>
    /// <returns></returns>
    public static (string flowId, string? parentId, string spanId) SetContext(string? flowId = null, string? parentId = null)
    {
        flowId ??= CreateFlowId();
        var spanId = CreateSpanId();

        CallContext.SetData(FlowIdName, flowId);
        CallContext.SetData(SpanIdName, spanId);

        if (parentId != null)
        {
            CallContext.SetData(ParentIdName, parentId);
        }
        return (flowId, parentId, spanId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static (string? flowId, string? parentId, string? spanId) GetContext()
    {
        return (
            CallContext.GetData(FlowIdName)?.ToString(),
            CallContext.GetData(ParentIdName)?.ToString(),
            CallContext.GetData(SpanIdName)?.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string?> GetContextAsDictionary()
    {
        var (contextFlowId, contextParentId, contextSpanId) = GetContext();
        return new Dictionary<string, string?>
        {
            { FlowIdName, contextFlowId },
            { ParentIdName, contextParentId },
            { SpanIdName, contextSpanId }
        }
        .Where(d => d.Value != null)
        .ToDictionary(k => k.Key, v => v.Value);

    }
}