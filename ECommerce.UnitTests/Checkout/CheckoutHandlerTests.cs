using System.Threading;
using ECommerce.Application.Abstractions;
using ECommerce.Application.Checkout.Commands;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.UnitTests.TestSupport;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECommerce.UnitTests.Checkout;

/// <summary>Pruebas del CheckoutHandler: éxito y rollback por stock insuficiente.</summary>
public sealed class CheckoutHandlerTests : IClassFixture<SqliteAppDbContextFactory>
{
    private readonly SqliteAppDbContextFactory _factory;
    public CheckoutHandlerTests(SqliteAppDbContextFactory factory) => _factory = factory;

    /// <summary>Implementación mínima de ITransactionService que no abre transacción (útil en un test).</summary>
    private sealed class TxPassthrough : ITransactionService
    {
        public Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct) => action(ct);
    }

    /// <summary>Implementación fake de IPublisher para pruebas que no publica eventos realmente.</summary>
    private sealed class FakePublisher : IPublisher
    {
        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) 
            where TNotification : INotification => Task.CompletedTask;
        
        public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    [Fact]
    public async Task Crea_pedido_descuenta_stock_y_limpia_carrito()
    {
        using var ctx = _factory.CreateContext();
        var (_, productId) = await SeedHelper.SeedProductAsync(ctx, price: 30m, stock: 10);
        var userId = Guid.NewGuid();

        var cart = new ECommerce.Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = userId };
        cart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3, UnitPrice = 30m });
        ctx.Carts.Add(cart); await ctx.SaveChangesAsync();

        var handler = new CheckoutHandler(ctx, new TxPassthrough(), new FakePublisher());
        var orderId = await handler.Handle(new CheckoutCommand(userId), CancellationToken.None);

        var order = await ctx.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Items.Single().Quantity.Should().Be(3);
        (await ctx.Products.FindAsync(productId))!.Stock.Should().Be(7);
        (await ctx.Carts.Include(c => c.Items).FirstAsync(c => c.UserId == userId)).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Hace_rollback_si_no_hay_stock()
    {
        using var ctx = _factory.CreateContext();
        var (_, productId) = await SeedHelper.SeedProductAsync(ctx, price: 30m, stock: 2);
        var userId = Guid.NewGuid();

        var cart = new ECommerce.Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = userId };
        cart.Items.Add(new CartItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3, UnitPrice = 30m });
        ctx.Carts.Add(cart); await ctx.SaveChangesAsync();

        // Usa TransactionService real para probar transacción/rollback
        var tx = new TransactionService(ctx);
        var handler = new CheckoutHandler(ctx, tx, new FakePublisher());

        var act = () => handler.Handle(new CheckoutCommand(userId), CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Stock insuficiente*");

        // Refresca el contexto para asegurar que leemos el estado actual después del rollback
        ctx.ChangeTracker.Clear();
        (await ctx.Products.FindAsync(productId))!.Stock.Should().Be(2);
        (await ctx.Orders.CountAsync()).Should().Be(0);
        (await ctx.Carts.Include(c => c.Items).FirstAsync(c => c.UserId == userId)).Items.Should().HaveCount(1);
    }
}
