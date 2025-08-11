using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;

namespace ECommerce.UnitTests.TestSupport;

/// <summary>Utilidades de seeding para pruebas.</summary>
public static class SeedHelper
{
    /// <summary>Crea una categoría y un producto activo con precio/stock dados.</summary>
    public static async Task<(Guid categoryId, Guid productId)> SeedProductAsync(
        AppDbContext ctx, decimal price = 50m, int stock = 100)
    {
        var cat = new Category { Id = Guid.NewGuid(), Name = "Test" };
        var prod = new Product { Id = Guid.NewGuid(), Name = "Test Product", Price = price, Stock = stock, CategoryId = cat.Id, IsActive = true };
        ctx.Categories.Add(cat);
        ctx.Products.Add(prod);
        await ctx.SaveChangesAsync();
        return (cat.Id, prod.Id);
    }
}
