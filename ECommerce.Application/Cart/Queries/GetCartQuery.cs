using ECommerce.Application.Abstractions;
using ECommerce.Application.Cart.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Cart.Queries;

/// <summary>Petición para obtener el carrito de un usuario.</summary>
/// <param name="UserId">Identificador del usuario autenticado.</param>
public sealed record GetCartQuery(Guid UserId) : IRequest<CartDto>;

/// <summary>Manejador que retorna el carrito del usuario, creándolo vacío si no existe.</summary>
public sealed class GetCartHandler : IRequestHandler<GetCartQuery, CartDto>
{
    private readonly IApplicationDbContext _db;
    public GetCartHandler(IApplicationDbContext db) => _db = db;

    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (cart is null)
        {
            cart = new Domain.Entities.Cart { Id = Guid.NewGuid(), UserId = request.UserId };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct);
        }

        // Unir con Products para nombres
        var productNames = await _db.Products
            .Where(p => cart.Items.Select(i => i.ProductId).Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p.Name, ct);

        var items = cart.Items
            .Select(i => new CartItemDto(
                i.ProductId,
                productNames.GetValueOrDefault(i.ProductId, "Producto"),
                i.UnitPrice,
                i.Quantity,
                i.UnitPrice * i.Quantity))
            .ToList();

        return new CartDto(cart.Id, cart.UserId, cart.UpdatedAt, items.Sum(x => x.Subtotal), items);
    }
}
