using Microsoft.EntityFrameworkCore;

namespace ShopStewardHub.DigitalTwin.Infrastructure.Database;

/// <summary>
/// Entity Framework DbContext for time-series database (TimescaleDB)
/// </summary>
public class TimeSeriesDbContext : DbContext
{
    public TimeSeriesDbContext(DbContextOptions<TimeSeriesDbContext> options) : base(options)
    {
    }

    // Note: For time-series data, we'll primarily use raw SQL queries
    // EF Core is configured here mainly for connection management
    // Actual queries will be in the TelemetryService using Dapper or raw ADO.NET
}
