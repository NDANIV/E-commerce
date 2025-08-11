using ECommerce.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Notifications;

/// <summary>
/// Implementación de ejemplo para el envío de correos electrónicos.
/// Actualmente solo escribe en logs. Sustituye por SMTP/SendGrid en producción.
/// </summary>
public sealed class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;

    /// <summary>
    /// Crea el emisor de emails.
    /// </summary>
    /// <param name="logger">Logger para registrar el envío.</param>
    public EmailSender(ILogger<EmailSender> logger) => _logger = logger;

    /// <inheritdoc />
    public Task SendAsync(string to, string subject, string body, CancellationToken ct = default)
    {
        _logger.LogInformation("EMAIL -> To: {To}; Subject: {Subject}\n{Body}", to, subject, body);
        return Task.CompletedTask;
    }
}
