using System.Threading;
using ECommerce.Application.Cart.Commands;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.UnitTests.TestSupport;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.UnitTests.Cart;

/// <summary>Pruebas del UpdateCartItemQtyHandler.</summary>
public sealed class UpdateCartItemQtyHandlerTests : IClassFixture<SqliteAppDbContextFactory>
{
    private readonly SqliteAppDbContextFactory _factory;
    public UpdateCartItemQtyHandlerTests(SqliteAppDbContextFactory factory) => _factory = factory;

    [Fact]
    public async Task Actualiza_cantidad_y_precio()
    {
        using var ctx = _factory.CreateContext();
        var (_, productId) = await SeedHelper.SeedProductAsync(ctx, price: 15m);
        var userId = Guid.NewGuid();

        var cart = new Cart { Id = Guid.NewGuid(), UserId = userId };
        cart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 1, UnitPrice = 10m });
        ctx.Carts.Add(cart); await ctx.SaveChangesAsync();

        (await ctx.Products.FindAsync(productId))!.Price = 20m; await ctx.SaveChangesAsync();

        var handler = new UpdateCartItemQtyHandler(ctx);
        await handler.Handle(new UpdateCartItemQtyCommand(userId, productId, 5), CancellationToken.None);

        var item = (await ctx.Carts.Include(c => c.Items).FirstAsync(c => c.UserId == userId)).Items.Single();
        item.Quantity.Should().Be(5);
        item.UnitPrice.Should().Be(20m);
    }
}
