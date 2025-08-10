using ECommerce.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Cart.Commands;

/// <summary>Limpia todos los ítems del carrito.</summary>
public sealed record ClearCartCommand(Guid UserId) : IRequest;

public sealed class ClearCartHandler : IRequestHandler<ClearCartCommand>
{
    private readonly IApplicationDbContext _db;
    public ClearCartHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);
        if (cart is null) return Unit.Value;

        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
