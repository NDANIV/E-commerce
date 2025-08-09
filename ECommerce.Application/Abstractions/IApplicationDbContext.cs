using ECommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Abstractions;

/// <summary>
/// Contrato mínimo de persistencia que la capa Application necesita.
/// No expone detalles de EF; sólo colecciones y SaveChanges.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Conjunto de productos. Implementado por la capa de infraestructura.
    /// </summary>
    DbSet<Product> Products { get; }

    /// <summary>
    /// Conjunto de categorías. Implementado por la capa de infraestructura.
    /// </summary>
    DbSet<Category> Categories { get; }

    /// <summary>
    /// Conjunto de pedidos. Implementado por la capa de infraestructura.
    /// </summary>
    DbSet<Order> Orders { get; }

    /// <summary>
    /// Conjunto de ítems de pedido. Implementado por la capa de infraestructura.
    /// </summary>
    DbSet<OrderItem> OrderItems { get; }

    /// <summary>
    /// Persiste los cambios pendientes en la unidad de trabajo actual.
    /// </summary>
    /// <param name="ct">Token de cancelación para operaciones asíncronas.</param>
    /// <returns>Número de entidades afectadas.</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}