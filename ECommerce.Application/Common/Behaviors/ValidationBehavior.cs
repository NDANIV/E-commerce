using FluentValidation;
using MediatR;

namespace ECommerce.Application.Common.Behaviors;

/// <summary>
/// Pipeline de MediatR que ejecuta las validaciones de FluentValidation
/// para un <typeparamref name="TRequest"/> antes de invocar su Handler.
/// </summary>
/// <typeparam name="TRequest">
/// Tipo de solicitud que debe implementar <see cref="IRequest{TResponse}"/>.
/// </typeparam>
/// <typeparam name="TResponse">
/// Tipo de respuesta devuelta por el Handler de la solicitud.
/// </typeparam>
/// <remarks>
/// Esta implementación asume la firma de MediatR clásica (v10/11) donde
/// <c>IPipelineBehavior&lt;TRequest,TResponse&gt;</c> opera con <c>IRequest&lt;TResponse&gt;</c>.
/// Si migras a MediatR v12 y usas otra firma, ajusta la restricción.
/// </remarks>
public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Crea el behavior con los validadores disponibles para <typeparamref name="TRequest"/>.
    /// </summary>
    /// <param name="validators">Colección de validadores registrados en DI.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    /// <summary>
    /// Ejecuta las validaciones y, si son exitosas, continúa con el siguiente paso del pipeline.
    /// </summary>
    /// <param name="request">Solicitud a validar.</param>
    /// <param name="next">Delegado para invocar el siguiente componente del pipeline/handler.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>La respuesta de tipo <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="ValidationException">
    /// Se lanza cuando una o más reglas de validación fallan.
    /// </exception>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = new List<FluentValidation.Results.ValidationFailure>();

            foreach (var validator in _validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                if (!result.IsValid)
                    failures.AddRange(result.Errors);
            }

            if (failures.Count != 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}