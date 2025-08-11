using ECommerce.Application.Abstractions;
using ECommerce.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Orders.Notifications;

/// <summary>
/// Maneja el evento <see cref="OrderPlacedEvent"/> para notificar al usuario.
/// Envía un email de confirmación.
/// </summary>
public sealed class OrderPlacedEventHandler : INotificationHandler<OrderPlacedEvent>
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailSender _email;

    /// <summary>
    /// Crea una instancia del handler de notificaciones.
    /// </summary>
    /// <param name="db">Acceso a datos para obtener información adicional del pedido/usuario.</param>
    /// <param name="email">Servicio de envío de emails.</param>
    public OrderPlacedEventHandler(IApplicationDbContext db, IEmailSender email)
    {
        _db = db;
        _email = email;
    }

    /// <summary>
    /// Ejecuta la notificación de pedido creado.
    /// </summary>
    /// <param name="notification">Evento con los datos del pedido.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
    {
        // 1) Buscar email del usuario 
       
        var userEmail = await LookupUserEmailAsync(notification.UserId, cancellationToken);

        // 2) Construir mensaje
        var subject = $"Confirmación de pedido #{notification.OrderId}";
        var body =
            $"¡Gracias por tu compra!\n\n" +
            $"Pedido: {notification.OrderId}\n" +
            $"Total: {notification.Total:C}\n\n" +
            $"Te avisaremos cuando se actualice el estado del pedido.\n";

        // 3) Enviar
        await _email.SendAsync(userEmail, subject, body, cancellationToken);
    }

    /// <summary>
    /// Ejemplo de obtención de email. En una implementación real,
    /// agrega IUsersReadService en Application y resuélvelo en Infrastructure (contra ASP.NET Identity).
    /// </summary>
    private Task<string> LookupUserEmailAsync(Guid userId, CancellationToken ct)
    {
        // TODO: Reemplazar por servicio real. Por ahora.
        // Como ejemplo, devolvemos un correo de desarrollo:
        return Task.FromResult("customer@shop.local");
    }
}
