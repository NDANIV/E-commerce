namespace ECommerce.Application.Cart.Dtos;

/// <summary>
/// Representación segura de un carrito para consumo externo.
/// </summary>
public sealed record CartDto(Guid Id, Guid UserId, DateTime UpdatedAt, decimal Total, List<CartItemDto> Items);

/// <summary>
/// Ítem del carrito proyectado a DTO.
/// </summary>
public sealed record CartItemDto(Guid ProductId, string Name, decimal UnitPrice, int Quantity, decimal Subtotal);