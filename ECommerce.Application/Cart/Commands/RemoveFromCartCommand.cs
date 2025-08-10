using ECommerce.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Cart.Commands;

/// <summary>Elimina un producto del carrito.</summary>
public sealed record RemoveFromCartCommand(Guid UserId, Guid ProductId) : IRequest;

public sealed class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand>
{
    private readonly IApplicationDbContext _db;
    public RemoveFromCartHandler(IApplicationDbContext db) => _db = db;

    public async Task<Unit> Handle(RemoveFromCartCommand request, CancellationToken ct)
    {
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == request.UserId, ct)
                   ?? throw new KeyNotFoundException("Carrito no encontrado.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == request.ProductId)
                   ?? throw new KeyNotFoundException("Ítem no encontrado.");

        _db.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
