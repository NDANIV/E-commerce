using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// Utilidad para poblar datos iniciales de la aplicación:
/// roles, usuario administrador y (opcional) datos semilla del catálogo.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Crea roles obligatorios, un usuario administrador de desarrollo
    /// y datos de catálogo mínimos si aún no existen.
    /// </summary>
    /// <param name="services">Proveedor de servicios para resolver dependencias.</param>
    public static async Task SeedAsync(IServiceProvider services)
    {
        // Usamos un scope porque creamos servicios con tiempo de vida Scoped
        using var scope = services.CreateScope();

        var ctx   = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

        // ===== 1) Migrar BD si falta (seguro en desarrollo) =====
        await ctx.Database.MigrateAsync();

        // ===== 2) Roles base =====
        foreach (var roleName in new[] { "Admin", "Customer" })
        {
            if (!await roles.RoleExistsAsync(roleName))
                await roles.CreateAsync(new IdentityRole<Guid>(roleName));
        }

        // ===== 3) Usuario Admin (solo si no existe) =====
        const string adminEmail = "admin@shop.local";
        const string adminPass  = "Admin123!"; // Solo dev. Usa secretos/variables en prod.

        var admin = await users.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            var create = await users.CreateAsync(admin, adminPass);
            if (create.Succeeded)
                await users.AddToRoleAsync(admin, "Admin");
            else
                throw new InvalidOperationException("No se pudo crear el usuario admin: " +
                                                    string.Join("; ", create.Errors.Select(e => e.Description)));
        }
        else
        {
            // Asegura que tenga rol Admin
            var userRoles = await users.GetRolesAsync(admin);
            if (!userRoles.Contains("Admin"))
                await users.AddToRoleAsync(admin, "Admin");
        }

        // ===== 4) Datos de catálogo (opcionales, para probar) =====
        if (!await ctx.Categories.AnyAsync())
        {
            var cat = new Category { Id = Guid.NewGuid(), Name = "General" };
            ctx.Categories.Add(cat);

            ctx.Products.Add(new Product
            {
                Id = Guid.NewGuid(),
                Name = "Producto Demo",
                Description = "Ejemplo para pruebas de catálogo.",
                Price = 49.90m,
                Stock = 100,
                CategoryId = cat.Id,
                IsActive = true
            });

            await ctx.SaveChangesAsync();
        }
    }
}
