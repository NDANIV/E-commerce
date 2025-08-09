using ECommerce.Application.Common;
using ECommerce.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Application;

/// <summary>
/// Métodos de extensión para registrar servicios de la capa Application
/// (MediatR, validadores y pipeline de validación).
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registra Handlers de MediatR, validadores de FluentValidation, y el
    /// Pipeline de validación global. Debe llamarse desde la capa WebApi.
    /// </summary>
    /// <param name="services">Contenedor de dependencias.</param>
    /// <returns>El mismo contenedor, para encadenar registros.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(ApplicationAssemblyMarker).Assembly;

        // Usa SOLO una de estas líneas según tu versión de MediatR:

        // ► MediatR v11 o anterior
        services.AddMediatR(assembly);

        // ► MediatR v12+ (si se actualiza):
        // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation: registra todos los validators del ensamblado
        services.AddValidatorsFromAssembly(assembly);

        // Pipeline global de validación
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}

