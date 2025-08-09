using FluentValidation;
using MediatR;

namespace ECommerce.Application.Common.Behaviors;

/// <summary>
/// Pipeline de MediatR que ejecuta validaciones de FluentValidation
/// antes de invocar el Handler correspondiente.
/// </summary>
/// <typeparam name="TRequest">Tipo de mensaje de entrada (Command/Query).</typeparam>
/// <typeparam name="TResponse">Tipo de respuesta del Handler.</typeparam>
public sealed class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    /// <summary>
    /// Crea el behavior con los validadores inyectados para el tipo TRequest.
    /// </summary>
    /// <param name="validators">Colección de validadores encontrados por DI.</param>
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    /// <inheritdoc />
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