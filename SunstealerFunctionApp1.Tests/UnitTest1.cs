using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OpenTelemetry.Metrics;
using Sunstealer.FunctionApp1.Functions;
using Sunstealer.FunctionApp1.Models;
using Sunstealer.FunctionApp1.Services;
using System.Diagnostics.Metrics;
using System.Text.Json;

namespace Sunslealer.FunctionApp1.Tests;

public class UnitTest1
{
    private readonly IApplicationService _application;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
    private readonly ILogger<ApplicationService> _loggerApplicationService;
    private readonly ILogger<Functions> _loggerFunctions;
    private readonly IMeterFactory _meterFactory;
    private readonly MeterProvider _meterProvider;
    private readonly ServiceCollection _ServiceCollection;

    public UnitTest1()
    {
        _ServiceCollection = new ServiceCollection();
        _ServiceCollection.AddLogging();
        var serviceProvider = _ServiceCollection.BuildServiceProvider();

        // ajm: 
        _configuration = new ConfigurationBuilder()
            .AddJsonFile(@"C:\Users\ajm\Documents\projects\ajm.azure\SunstealerFunctionApp1\local.settings.json")
            .Build();

        // ajm:
        _loggerApplicationService = new Mock<ILogger<ApplicationService>>().Object;
        _loggerFunctions = new Mock<ILogger<Functions>>().Object;

        // ajm:
        var mockMeterFactory = new Mock<IMeterFactory>();
        mockMeterFactory
            .Setup(f => f.Create(It.IsAny<MeterOptions>()))
            .Returns(new Meter("Sunstealer.FunctionApp1.Worker"));
        _meterFactory = mockMeterFactory.Object;
        _meterProvider = new Mock<MeterProvider>().Object;

        // ajm:
        var data = new List<Table1>
        {
            new Table1 { UUID = 1, Date1 = DateTime.UtcNow, Encrypted1 = "", Number1 = 1, Text1 = "One" },
            new Table1 { UUID = 2, Date1 = DateTime.UtcNow, Encrypted1 = "", Number1 = 2, Text1 = "Two" }
        }.AsQueryable();

        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        using (var context = new ApplicationDbContext(options))
        {
            context.Database.EnsureCreated();
            context.Table1.Add(new Table1 { UUID = 1, Date1 = DateTime.UtcNow, Encrypted1 = "", Number1 = 1, Text1 = "One" });
            context.Table1.Add(new Table1 { UUID = 2, Date1 = DateTime.UtcNow, Encrypted1 = "", Number1 = 2, Text1 = "Two" });
            context.Table1.Add(new Table1 { UUID = 3, Date1 = DateTime.UtcNow, Encrypted1 = "", Number1 = 3, Text1 = "Three" });
            context.SaveChanges();
        }

        var mockDbContextFactory = new Mock<IDbContextFactory<ApplicationDbContext>>();
        mockDbContextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new ApplicationDbContext(options));

        _dbContextFactory = mockDbContextFactory.Object;

        // ajm: _application = new Mock<IApplicationService>().Object;
        _application = new ApplicationService(_configuration, _loggerApplicationService);
    }

    /// <summary>
    /// Test
    /// </summary>
    [Fact]
    public async Task TestDB()
    {
        // arrange
        var context = new DefaultHttpContext();
        var request = context.Request;

        var function = new Functions(_application, _configuration, _dbContextFactory, _loggerFunctions, _meterFactory, _meterProvider);

        // act
        var result = function.EntityFrameworkLinq(request) as ObjectResult;

        List<Table1>? table1 = result?.Value as List<Table1>;

        // assert
        Assert.True(table1?[0].Text1 == "One");
    }

    /// <summary>
    /// 
    /// </summary>
    [Fact]
    public void TestResponseObject()
    {
        // arrange
        var context = new DefaultHttpContext();
        var request = context.Request;

        var function = new Functions(_application, _configuration, _dbContextFactory, _loggerFunctions, _meterFactory, _meterProvider);

        // act
        IActionResult result = function.Log(request);

        if (Configuration.ChunkResponse == 0)
        {
            // assert
            Assert.IsType<ContentResult>(result);
            Assert.NotNull(result);
        }
        else
        {
            // assert
            Assert.IsType<ObjectResult>(result);
            Assert.NotNull(result);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task TestResponseHeaders()
    {
        // arrange
        FixtureFunctionHost fixture = new FixtureFunctionHost();
        await fixture.Initialize();

        // act
        var response = await fixture._client.GetAsync($"{fixture._baseUrl}/api/Log");

        var headers = response.Headers;

        // assert
        Assert.True(headers.Contains("Server"));

        fixture.Dispose();
    }
}
