using ECommerce.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Cart.Commands;

/// <summary>Actualiza la cantidad de un ítem del carrito.</summary>
public sealed record UpdateCartItemQtyCommand(Guid UserId, Guid ProductId, int Quantity) : IRequest;

public sealed class UpdateCartItemQtyValidator : AbstractValidator<UpdateCartItemQtyCommand>
{
    public UpdateCartItemQtyValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}

public sealed class UpdateCartItemQtyHandler : IRequestHandler<UpdateCartItemQtyCommand>
{
    private readonly IApplicationDbContext _db;
    public UpdateCartItemQtyHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(UpdateCartItemQtyCommand request, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct)
            ?? throw new KeyNotFoundException("Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
            ?? throw new KeyNotFoundException("Ítem no encontrado.");

        // Refrescar precio actual del producto
        var product = await _db.Products.FirstAsync(p => p.Id == request.ProductId, ct);
        item.Quantity = request.Quantity;
        item.UnitPrice = product.Price;

        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
