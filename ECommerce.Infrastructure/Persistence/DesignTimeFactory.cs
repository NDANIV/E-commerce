using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Factory usada SOLO por las herramientas de EF Core en tiempo de diseño.
/// Evita depender de Program.cs y de la conexión real.
/// </summary>
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Usa una cadena local o de env; NO uses AutoDetect aquí
        var cs = Environment.GetEnvironmentVariable("ECOMMERCE_CS") 
                 ?? "server=localhost;port=3306;database=ecommerce;user=ecom_user;password=secret123;TreatTinyAsBoolean=true;SslMode=None;AllowPublicKeyRetrieval=True";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            // se usa versión explícita para NO abrir conexión:
            .UseMySql(cs, new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        return new AppDbContext(options);
    }
}