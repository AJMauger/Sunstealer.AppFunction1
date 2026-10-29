using Microsoft.EntityFrameworkCore;

namespace Sunstealer.FunctionApp1.Services;

/// <summary>
/// 
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        /* ajm: var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
                .AddConsole();
        });

        optionsBuilder
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging(false);*/
    }

    /// <summary>
    /// 
    /// </summary>
    public DbSet<Models.Table1> Table1 { get; set; }
}
