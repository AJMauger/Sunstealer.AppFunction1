using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Primitives;
using System.Net;

namespace Sunstealer.FunctionApp1.Middleware;

/// <summary>
/// 
/// </summary>
public class ContextMiddleware : IFunctionsWorkerMiddleware
{
    /// <summary>
    /// ContextMiddleware Invoke is called by the dependency pipeline for each HTTP Request.  
    /// A <see cref="Flow.CreateFlowId"/> and <see cref="Flow.CreateSpanId"/> are assigned and the API handler called by <b>await next(context);</b>.
    /// If the API throws an exception it is caught and a default HTTP Resopnse dispatched to the HTTP client.
    /// </summary>
    /// <param name="context">The HTTP Request context.</param>
    /// <param name="next">The API Handler.</param>
    /// <returns></returns>
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        StringValues flowId = string.Empty;
        StringValues spanId = string.Empty;

        context.GetHttpContext()?.Request.Headers.TryGetValue(Flow.FlowIdName, out flowId);
        context.GetHttpContext()?.Request.Headers.TryGetValue(Flow.SpanIdName, out spanId);

        Flow.SetContext(flowId, parentId: spanId);

        context.GetHttpContext()?.Response.OnStarting(state =>
        {
            foreach (var item in Flow.GetContextAsDictionary())
            {
                context.GetHttpContext()?.Response.Headers.Append(item.Key, item.Value);
            }
            return Task.CompletedTask;
        }, context);

        try
        {
            await next(context);
        }
        catch (Exception e)
        {
            var req = context.GetHttpContext()?.Request;
            if (req != null)
            {
                await Functions.Functions.FinalizeResponse(req, HttpStatusCode.InternalServerError, new { error = e.Message });
            }
            else
            {
                throw;
            }
        }
    }
}