namespace ECommerce.Application.Abstractions;

/// <summary>
/// Servicio de orquestación transaccional para operaciones multi‑paso.
/// Permite implementar transacciones en infraestructura sin acoplar a EF.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Ejecuta la función <paramref name="action"/> dentro de una transacción.
    /// Si falla, realiza rollback.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<CancellationToken, Task> action, CancellationToken ct);
}
