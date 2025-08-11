namespace ECommerce.Domain.Entities;

/// <summary>
/// Carrito de compras asociado a un usuario autenticado.
/// Persistido por usuario para soportar sesiones múltiples y recuperación.
/// </summary>
public class Cart
{
    /// <summary>Identificador del carrito.</summary>
    public Guid Id { get; set; }

    /// <summary>Id del usuario propietario del carrito.</summary>
    public Guid UserId { get; set; }

    /// <summary>Fecha de última actualización (UTC).</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Productos agregados al carrito.</summary>
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    /// <summary>Total calculado con los precios actuales de los ítems en el carrito.</summary>
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}

/// <summary>
/// Ítem del carrito con la cantidad y el precio actual del producto al momento de agregarlo.
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid CartId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>Cantidad solicitada por el usuario (>=1).</summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Precio de referencia guardado al añadir al carrito (puede revalidarse al checkout).
    /// Para simplificar, lo refrescaremos al actualizar cantidades.
    /// </summary>
    public decimal UnitPrice { get; set; }
}