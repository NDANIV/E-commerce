using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Checkout.Commands;

/// <summary>
/// Efectúa el checkout del carrito del usuario:
/// valida stock, congela precios, crea Order, descuenta inventario y limpia el carrito.
/// </summary>
public sealed record CheckoutCommand(Guid UserId) : IRequest<Guid>;

public sealed class CheckoutValidator : AbstractValidator<CheckoutCommand>
{
    public CheckoutValidator() => RuleFor(x => x.UserId).NotEmpty();
}

public sealed class CheckoutHandler : IRequestHandler<CheckoutCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ITransactionService _tx;
    private readonly IPublisher _publisher;

    public CheckoutHandler(IApplicationDbContext db, ITransactionService tx, IPublisher publisher)
    {
        _db = db; _tx = tx; _publisher = publisher;
    }

    public async Task<Guid> Handle(CheckoutCommand request, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items)
                                  .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct)
                   ?? throw new InvalidOperationException("No hay carrito para el usuario.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("El carrito está vacío.");

        var orderId = Guid.NewGuid();

        await _tx.ExecuteInTransactionAsync(async _ =>
        {
            // Releer productos y validar stock actual
            var productIds = cart.Items.Select(i => i.ProductId).Distinct().ToList();
            var products = await _db.Products
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToDictionaryAsync(p => p.Id, ct);

            foreach (var it in cart.Items)
            {
                if (!products.TryGetValue(it.ProductId, out var p))
                    throw new InvalidOperationException($"Producto {it.ProductId} no disponible.");

                if (p.Stock < it.Quantity)
                    throw new InvalidOperationException($"Stock insuficiente para '{p.Name}'. Disponible: {p.Stock}.");

                // Descuento de stock
                p.Stock -= it.Quantity;
            }

            // Crear el pedido con precios *congelados al momento*
            var order = new Order
            {
                Id = orderId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                Items = cart.Items.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = orderId,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = products[ci.ProductId].Price // tomamos precio actual
                }).ToList()
            };

            _db.Orders.Add(order);

            // Limpiar carrito
            _db.CartItems.RemoveRange(cart.Items);
            cart.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
        }, ct);

        await _publisher.Publish(new OrderPlacedEvent(orderId, request.UserId, total), ct);

        return orderId;
    }
}

