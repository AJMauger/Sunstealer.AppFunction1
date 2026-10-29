using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Sunstealer.FunctionApp1.Middleware;
using Sunstealer.FunctionApp1.Services;

Console.WriteLine($"Folder: {System.IO.Directory.GetCurrentDirectory()}");

// ajm: Configuration.CreateCEK();

var builder = FunctionsApplication.CreateBuilder(args);

var worker = builder.ConfigureFunctionsWebApplication();
worker.UseMiddleware<ContextMiddleware>();

var credential = new DefaultAzureCredential();
var provider = new SqlColumnEncryptionAzureKeyVaultProvider(credential);
SqlConnection.RegisterColumnEncryptionKeyStoreProviders(
    new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>()
    {
        { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, provider }
    });

worker.Services
    .AddOpenTelemetry()
    // ajm: .UseAzureMonitorExporter()
    .WithMetrics(options =>
    {
        options.AddMeter("Sunstealer.FunctionApp1.Worker");
        options.AddAzureMonitorMetricExporter();

        options.AddAspNetCoreInstrumentation();
        options.AddHttpClientInstrumentation();
        options.AddRuntimeInstrumentation();
        options.AddSqlClientInstrumentation();
    })
    .WithTracing(options =>
    {
        options.AddSource("Sunstealer.FunctionApp1.Worker");
        options.AddAzureMonitorTraceExporter();

        options.AddAspNetCoreInstrumentation(options =>
        {
            options.RecordException = true;
        });
        // ajm: options.AddAzureSdkInstrumentation();
        // ajm: options.AddEntityFrameworkCoreInstrumentation();
        // ajm: options.AddGrpcClientInstrumentation();
        options.AddHttpClientInstrumentation(options =>
        {
            options.RecordException = true;
        });
        options.AddSqlClientInstrumentation(options =>
        {
            options.RecordException = true;
        });
    });

worker.Services
    .AddDbContextFactory<ApplicationDbContext>(options =>
    {
        var azureDbConnectionString = worker.Configuration.GetConnectionString("AzureDbConnectionString");
        options.UseSqlServer(azureDbConnectionString);
    });

builder.Services.AddLogging(options =>
{
    options.AddOpenTelemetry(options =>
    {
        options.IncludeScopes = true;
        // ajm: options.IncludeFormattedMessage = true;
    });
});

builder.Services.AddSingleton<IApplicationService, ApplicationService>();

builder.Logging.AddOpenTelemetry(options =>
{
    options.AddAzureMonitorLogExporter();
});

builder.Build().Run();

namespace Sunstealer.FunctionApp1
{

    /// <summary>
    /// DocFX needs this.
    /// </summary>
    public partial class Program
    {
    }
}