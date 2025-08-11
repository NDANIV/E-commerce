using ECommerce.Application.Abstractions;
using ECommerce.Application.Products.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Products.Queries;

/// <summary>
/// Petición para obtener el catálogo de productos con filtros opcionales.
/// </summary>
/// <param name="Search">Texto para filtrar por nombre (contiene).</param>
/// <param name="CategoryId">Id de la categoría a filtrar.</param>
public sealed record GetProductsQuery(string? Search, Guid? CategoryId) 
    : IRequest<List<ProductDto>>;

/// <summary>
/// Handler que ejecuta la lógica de obtención del catálogo de productos.
/// </summary>
public sealed class GetProductsHandler 
    : IRequestHandler<GetProductsQuery, List<ProductDto>>
{
    private readonly IApplicationDbContext _db;

    /// <summary>
    /// Crea una nueva instancia del handler.
    /// </summary>
    /// <param name="db">Contexto de aplicación (abstracción de persistencia).</param>
    public GetProductsHandler(IApplicationDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<List<ProductDto>> Handle(GetProductsQuery request, CancellationToken ct)
    {
        // Query base: sólo productos activos
        IQueryable<Domain.Entities.Product> query =
            _db.Products.AsNoTracking()
                        .Include(p => p.Category)
                        .Where(p => p.IsActive);

        // Filtro por búsqueda (contiene, case-sensitive/culture por defecto del proveedor)
        if (!string.IsNullOrWhiteSpace(request.Search))
            query = query.Where(p => p.Name.Contains(request.Search));

        // Filtro por categoría
        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        // Proyección a DTO (nunca devolvemos entidades)
        var result = await query
            .Select(p => new ProductDto(
                p.Id,
                p.Name,
                p.Price,
                p.Category.Name
            ))
            .ToListAsync(ct);

        return result;
    }
}