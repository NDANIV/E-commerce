using MediatR;

namespace ECommerce.Domain.Events;

/// <summary>
/// Evento emitido cuando un pedido se ha creado exitosamente.
/// </summary>
/// <param name="OrderId">Identificador del pedido recién creado.</param>
/// <param name="UserId">Identificador del usuario que efectuó el pedido.</param>
/// <param name="Total">Importe total del pedido, suma de sus ítems (cantidad * precio).</param>
public sealed record OrderPlacedEvent(Guid OrderId, Guid UserId, decimal Total) : INotification;
