using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Configurations.AppSettings.Extensions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using Sunstealer.FunctionApp1.Middleware;
using Sunstealer.FunctionApp1.Models;
using Sunstealer.FunctionApp1.Monitor;
using Sunstealer.FunctionApp1.Services;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Net;
using System.Web.Http;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Sunstealer.FunctionApp1.Functions;

/// <summary>
/// class
/// </summary>
public class Functions
{
    private readonly MeterProvider _meterProvider;
    // ajm: if (ObservableX<>) { pull on scrape } else { X<> = push }.
    private readonly ActivitySource _dependency;        // ajm: activity metrics
    private readonly Gauge<int> _gauge;                 // ajm: single final value X
    private readonly Histogram<long> _histograme;       // ajm: data _array (buckets), sum, count, min, max, and percentiles.
    private readonly Counter<int> _counter;             // ajm: single current value 0 ... X.
    private readonly UpDownCounter<int> _upDownCounter; // ajm: single current value (+X) / (-X) sum of all values.

    private readonly HttpClient httpClient = new HttpClient();
    private readonly IApplicationService? _application;
    private readonly IDbContextFactory<ApplicationDbContext>? _dbContextFactory;
    private readonly IConfiguration? _configuration;
    private readonly ILogger? _logger;


    // ajm: test
    List<int> _array = new List<int>();
    SequenceNumberService _sequenceNumberService = new();


    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// Functions contructor with dependency injection of Services used by the Triggers (API handlers).
    /// If a Service is null the constructor will throw an exception that will be processed by <see cref="ContextMiddleware" />.  
    /// </summary>
    /// <param name="application">Is a Singleton Service (only one, shared instance exists) used to manage global application resources.</param>
    /// <param name="configuration">Is the Microsoft Configuration Service with access to application environment variables, settings, and the ACS configuration.</param>
    /// <param name="dbContextFactory">Is a Factory Service used to create Database Comntexts (Entity Framework database connections)</param>
    /// <param name="logger">Is the Microsoft logger service which is extended (overridden) with <see cref="LoggerExtensions"/></param>
    /// <param name="meterFactory">Is a Factory Service used to create Open Telemetry Meters.</param>
    /// <param name="meterProvider">Is a Provider Object used to govern Open Telemetry.</param>
    /// <exception cref="Exception"></exception>
    public Functions(IApplicationService application, IConfiguration configuration, IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger<Functions> logger, IMeterFactory meterFactory, MeterProvider meterProvider)
    {
        _application = application ?? throw new Exception("application");
        _configuration = configuration ?? throw new Exception("configuration");
        _dbContextFactory = dbContextFactory ?? throw new Exception("dbContextFactory");
        _logger = logger ?? throw new Exception("logger");

        _logger.LogInformation($"Functions.Functions().");

        _meterProvider = meterProvider ?? throw new Exception("meterProvider");
        var meter = meterFactory.Create("Sunstealer.FunctionApp1.Worker");
        _upDownCounter = meter.CreateUpDownCounter<int>("updowncounter") ?? throw new Exception("updowncounter");
        _gauge = meter.CreateGauge<int>("gauge") ?? throw new Exception("gauge");
        _counter = meter.CreateCounter<int>("Counter", "count") ?? throw new Exception("counter");
        _histograme = meter.CreateHistogram<long>("Duration", "ms") ?? throw new Exception("duration");
        _dependency = new ActivitySource("Sunstealer.FunctionApp1.Worker") ?? throw new Exception("dependency");

        _logger.LogDebug("ILogger Debug Test.");
        _logger.LogInformation("ILogger Information Test.");
        _logger.LogWarning("ILogger Warning Test.");
        _logger.LogError("ILogger Error Test.");
        _logger.LogError(new Exception("AdamException"), "ILogger Exception Test.");

        File.Delete(@".\sequence_state.txt");
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("AlwaysEncrypted")]
    [OpenApiOperation(operationId: "Operation.Id: AlwaysEncrypted", Description = "Operation.Description: AlwaysEncrypted")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<Table1>), Description = "Description.Response", Example = typeof(List<Table1>))]
    public async Task<IActionResult> AlwaysEncrypted([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.AlwaysEncrypted().", string.Empty);

            var dbContext = _dbContextFactory?.CreateDbContext();

            var data = new List<Table1>();
            var now = DateTime.Now;
            if (dbContext != null)
            {
                string encrypted1 = "Encrypted";
                data = dbContext.Table1.Where(f => f.Encrypted1 == encrypted1).ToList();
                data.ForEach(f =>
                {
                    Console.WriteLine($"UUID: {f.UUID}, Encrypted1: {f.Encrypted1}, Date1: {f.Date1}, Number1: {f.Number1}, Text1: {f.Text1}");
                });
            }
            Logger.Instance.LogInformation($"Duration: {DateTime.Now - now}", string.Empty);
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.EntityFrameworkLinq()", string.Empty);
            return new InternalServerErrorResult();
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("EntityFrameworkLinq")]
    [OpenApiOperation(operationId: "EntityFrameworkLinq", Description = "EntityFrameworkLinq")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult EntityFrameworkLinq([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            Logger.Instance.LogInformation($"Functions.EntityFrameworkLinq().", string.Empty);

            List<Table1> data = new List<Table1>();
            using (var db = _dbContextFactory?.CreateDbContext())
            {
                var now = DateTime.Now;
                if (db != null)
                {
                    data = db.Table1.Where(f => f.UUID > 0).ToList();
                    data.ForEach(f => f.Number1++);
                    db.SaveChanges();
                }
                Logger.Instance.LogInformation($"Duration: {DateTime.Now - now}", string.Empty);
            }
            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            Logger.Instance.LogException(e, "Function1.EntityFrameworkLinq()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("GetConfiguration")]
    [OpenApiOperation(operationId: "GetConfiguration", Description = "GetConfiguration")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult GetConfiguration([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            string[] filter = new[]
            {
                "certificate", "clientid", "clientsecret", "connection", "key", "password", "private", "secret", "token", "username"
            };

            //// <summary>
            //// boop Logger
            //// </summary>
            //// <param name="req"></param>
            //// <returns></returns>
            _logger?.LogInformation("IConfiguration.AsEnumerable() start");
            var data = new Dictionary<string, string>();
            foreach (var i in _configuration?.AsEnumerable() ?? [])
            {
                string value = i.Value ?? "";
                if (!filter.Any(s => i.Key.Contains(s, StringComparison.OrdinalIgnoreCase)) && !filter.Any(s => value.Contains(s, StringComparison.OrdinalIgnoreCase)))
                {
                    value = value.Replace(_configuration.Get<string>("Subscription_Id"), "<subscription-id>");
                    _logger?.LogInformation($"{i.Key} = {i.Value}.");
                    data.Add(i.Key, value ?? "null");
                }
                else
                {
                    _logger?.LogInformation($"{i.Key} = [REDACTED].");
                    data.Add(i.Key, "[REDACTED]");
                }
            }
            _logger?.LogInformation("IConfiguration.AsEnumerable() end");

            return new OkObjectResult(data);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Function1.GetConfiguration()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("Log")]
    [OpenApiOperation(operationId: "Log", Description = "Log")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "Description.Response", Example = typeof(string))]
    public IActionResult Log([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        try
        {
            _logger?.LogInformation($"Functions.Log().");
            // ajm: return new OkObjectResult(Configuration.log);

            return FinalizeResult(req, HttpStatusCode.OK, Configuration.log);
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Function1.Log()", string.Empty);
            return new InternalServerErrorResult(); ;
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("TrackDependency")]
    [OpenApiOperation(operationId: "TrackDependency", Description = "TrackDependency")]
    public async Task<IActionResult> TrackDependency([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            using (var activity = _dependency.StartActivity("ExternalServiceName", ActivityKind.Client))
            {
                activity?.SetTag("peer.service", "RemoteApiSystem");

                int ms = Random.Shared.Next(1, 1000);
                await Task.Delay(ms);
                //// <summary>
                //// boop OpeTelemtry
                //// </summary>
                //// <param name="req"></param>
                //// <returns></returns>
                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            return new OkObjectResult("TrackDependency");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackDependency().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("TrackEvent")]
    [OpenApiOperation(operationId: "TrackEvent", Description = "TrackEvent")]
    public async Task<IActionResult> TrackEvent([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            var activity = Activity.Current;
            activity?.AddEvent(new ActivityEvent("CustomEvent", tags: new ActivityTagsCollection
            {
                { "customProperty", "value" }
            }));
            return new OkObjectResult("TrackEvent");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackEvent().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("TrackMetric")]
    [OpenApiOperation(operationId: "TrackMetric", Description = "TrackMetric")]
    public async Task<IActionResult> TrackMetric([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            int ms = Random.Shared.Next(1, 1000);
            await Task.Delay(ms);
            _histograme.Record(ms, new TagList { { "CustomDimension", "Duration" } });
            _counter.Add(1, new TagList { { "CustomDimension", "Counter" } });
            _meterProvider.ForceFlush(5000);
            return new OkObjectResult("TrackMetric");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.TrackMetric().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("Test")]
    [OpenApiOperation(operationId: "Test", Description = "Container1")]
    public async Task<IActionResult> Test([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        try
        {
            TestSequeceNumberService(true);

            Console.WriteLine("CRASH");

            TestSequeceNumberService(false);

            return new OkObjectResult("Test");
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Function.Test().");
            return new ObjectResult(new
            {
                error = "Internal Server Error",
                message = $"{e.Message} - {e.StackTrace}"
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Function("Throw")]
    [OpenApiOperation(operationId: "Throw", Description = "Throw")]
    public async Task<IActionResult> Throw([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
    {
        throw new Exception("Adam Exception");
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <param name="statusCode"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static async Task FinalizeResponse(HttpRequest req, HttpStatusCode statusCode, object payload)
    {
        byte[] jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(payload);
        var response = req.HttpContext.Response;
        if (Configuration.ChunkResponse == 0)
        {
            response.Headers.ContentLength = jsonBytes.Length;
        }
        response.ContentType = "application/json";

        // ajm: addd headers here 

        response.StatusCode = (int)statusCode;
        await System.Text.Json.JsonSerializer.SerializeAsync(response.Body, payload);
        await response.Body.FlushAsync();
    }

    // ajm: ---------------------------------------------------------------------------------------
    /// <summary>
    /// 
    /// </summary>
    /// <param name="req"></param>
    /// <param name="statusCode"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static ActionResult FinalizeResult(HttpRequest req, HttpStatusCode statusCode, object payload)
    {
        ActionResult? result;
        if (Configuration.ChunkResponse == 0)
        {
            // ajm: content-length
            result = new ContentResult()
            {
                Content = System.Text.Json.JsonSerializer.Serialize(payload),
                StatusCode = (int)statusCode,
                ContentType = "application/json"
            };
        }
        else
        {
            // ajm: encoding: chunked
            result = new ObjectResult(payload)
            {
                StatusCode = (int)statusCode,
            };
        }

        return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="crash"></param>
    /// <exception cref="Exception"></exception>
    public void TestSequeceNumberService(bool crash)
    {
        int start = 0;
        if (!crash)
        {
            start = 200_001;
        } 

        for (int i = start; i < 999_999; i++)
        {
            int j = _sequenceNumberService.Next();
            Console.WriteLine($"[{i}]={j}");
            if (_array.Contains(j))
            {
                throw new Exception($"SequenceNumberService.Next() {j} repeated.");
            }
            _array.Add(j);

            if (crash && i==250_000)
            {
                break;
            }
        }
    }
}

