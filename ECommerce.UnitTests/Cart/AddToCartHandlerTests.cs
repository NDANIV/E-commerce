using System.Threading;
using ECommerce.Application.Cart.Commands;
using ECommerce.Infrastructure.Persistence;
using ECommerce.UnitTests.TestSupport;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.UnitTests.Cart;

/// <summary>Pruebas del AddToCartHandler.</summary>
public sealed class AddToCartHandlerTests : IClassFixture<SqliteAppDbContextFactory>
{
    private readonly SqliteAppDbContextFactory _factory;
    public AddToCartHandlerTests(SqliteAppDbContextFactory factory) => _factory = factory;

    [Fact]
    public async Task Agrega_item_nuevo_si_no_existia()
    {
        using var ctx = _factory.CreateContext();
        var (_, productId) = await SeedHelper.SeedProductAsync(ctx, price: 10m, stock: 50);
        var userId = Guid.NewGuid();

        var handler = new AddToCartHandler(ctx);
        await handler.Handle(new AddToCartCommand(userId, productId, 2), CancellationToken.None);

        var cart = await ctx.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        cart!.Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2 && i.UnitPrice == 10m);
    }

    [Fact]
    public async Task Incrementa_cantidad_y_refresca_precio()
    {
        using var ctx = _factory.CreateContext();
        var (_, productId) = await SeedHelper.SeedProductAsync(ctx, price: 10m);
        var userId = Guid.NewGuid();

        var handler = new AddToCartHandler(ctx);
        await handler.Handle(new AddToCartCommand(userId, productId, 1), CancellationToken.None);

        // cambia precio
        (await ctx.Products.FindAsync(productId))!.Price = 12m;
        await ctx.SaveChangesAsync();

        await handler.Handle(new AddToCartCommand(userId, productId, 3), CancellationToken.None);

        var item = (await ctx.Carts.Include(c => c.Items).FirstAsync(c => c.UserId == userId)).Items.Single();
        item.Quantity.Should().Be(4);
        item.UnitPrice.Should().Be(12m);
    }
}
