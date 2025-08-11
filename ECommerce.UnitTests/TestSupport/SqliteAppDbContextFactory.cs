using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.UnitTests.TestSupport;

/// <summary>
/// Crea un AppDbContext sobre una conexión SQLite in-memory única para cada test.
/// </summary>
public sealed class SqliteAppDbContextFactory : IDisposable
{


    /// <summary>
    /// Crea un nuevo AppDbContext usando una nueva conexión en memoria para cada test.
    /// </summary>
    public AppDbContext CreateContext()
    {
        var connectionString = $"DataSource=test_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        var conn = new SqliteConnection(connectionString);
        conn.Open();

        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new AppDbContext(opts);
        ctx.Database.EnsureCreated(); // crea el esquema en la conexión en memoria
        
       
        ctx.Database.GetDbConnection().StateChange += (_, e) =>
        {
            if (e.CurrentState == System.Data.ConnectionState.Closed)
                conn.Dispose();
        };
        
        return ctx;
    }

    public void Dispose()
    {
        // Nothing to dispose - each context manages its own connection
    }
}
