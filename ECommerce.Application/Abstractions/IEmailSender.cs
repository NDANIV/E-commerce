namespace ECommerce.Application.Abstractions;

/// <summary>
/// Abstracción para envío de mensajes por correo electrónico.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Envía un correo de texto plano a un destinatario.
    /// </summary>
    /// <param name="to">Dirección destino.</param>
    /// <param name="subject">Asunto del mensaje.</param>
    /// <param name="body">Cuerpo del mensaje (texto plano).</param>
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}
