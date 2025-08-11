using ECommerce.Application.Abstractions;
using ECommerce.Infrastructure.Auth;
using ECommerce.Infrastructure.Identity;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure;

/// <summary>
/// Métodos de extensión para registrar servicios de infraestructura:
/// EF Core (MySQL), ASP.NET Identity y servicios auxiliares (JWT, etc.).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra el DbContext, Identity (usuarios/roles) y servicios de infraestructura.
    /// </summary>
    /// <param name="services">Contenedor de dependencias.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>El contenedor para encadenar llamadas.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        var connectionString = configuration.GetConnectionString("Default")
            ?? configuration["ConnectionStrings:Default"]
            ?? throw new InvalidOperationException("ConnectionStrings:Default no configurado.");

        // DbContext con Pomelo MySQL
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), mySql =>
            {
                // Config opcionales: retardos, comportamiento de batch, etc.
                mySql.EnableRetryOnFailure(5);
            });
        });

        // Identity con Guid
        services.AddIdentity<AppUser, IdentityRole<Guid>>(opt =>
        {
            // Políticas básicas de password 
            opt.Password.RequireDigit = true;
            opt.Password.RequiredLength = 6;
            opt.Password.RequireNonAlphanumeric = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireLowercase = false;

            // Opcional: confirmar correo antes de iniciar sesión
            opt.SignIn.RequireConfirmedAccount = false;
        })
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        // Exponer IApplicationDbContext como AppDbContext (puerto -> implementación)
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Transaction service
        services.AddScoped<ITransactionService, TransactionService>();
        
        // JWT service
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Email sender
        services.AddScoped<IEmailSender, EmailSender>();



        return services;
    }
}
