using ECommerce.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Cart.Commands;

/// <summary>Agrega un producto al carrito del usuario (o incrementa su cantidad).</summary>
public sealed record AddToCartCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest;

public sealed class AddToCartValidator : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class AddToCartHandler : IRequestHandler<AddToCartCommand>
{
    private readonly IApplicationDbContext _db;
    public AddToCartHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(AddToCartCommand request, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive, ct)
                      ?? throw new KeyNotFoundException("Producto no encontrado o inactivo.");

        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        if (cart is null)
        {
            cart = new Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = request.UserId };
            _db.Carts.Add(cart);
        }

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId);
        if (item is null)
        {
            cart.Items.Add(new Domain.Entities.CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart.Id,
                ProductId = product.Id,
                Quantity = request.Quantity,
                UnitPrice = product.Price
            });
        }
        else
        {
            item.Quantity += request.Quantity;
            item.UnitPrice = product.Price; // refrescamos precio actual
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
