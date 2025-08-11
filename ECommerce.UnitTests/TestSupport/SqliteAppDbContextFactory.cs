using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.UnitTests.TestSupport;

/// <summary>
/// Crea un AppDbContext sobre una conexión SQLite in-memory compartida,
/// ideal para pruebas unitarias con comportamiento relacional real.
/// </summary>
public sealed class SqliteAppDbContextFactory : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");

    public SqliteAppDbContextFactory() => _conn.Open();

    /// <summary>
    /// Crea un nuevo AppDbContext usando la misma conexión en memoria.
    /// </summary>
    public AppDbContext CreateContext()
    {
        var opts = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_conn)
            .Options;

        var ctx = new AppDbContext(opts);
        ctx.Database.EnsureCreated(); // crea el esquema en la conexión en memoria
        return ctx;
    }

    public void Dispose() => _conn.Dispose();
}
