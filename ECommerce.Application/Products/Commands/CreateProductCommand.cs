using ECommerce.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Application.Products.Commands;

/// <summary>
/// Comando para crear un nuevo producto dentro de una categoría existente.
/// </summary>
public sealed record CreateProductCommand(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    Guid CategoryId
) : IRequest<Guid>; // devuelve el Id del nuevo producto

/// <summary>
/// Handler que crea un nuevo producto validando existencia de categoría y reglas básicas.
/// </summary>
public sealed class CreateProductHandler 
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _db;

    /// <summary>
    /// Instancia el handler de creación de producto.
    /// </summary>
    /// <param name="db">Contexto de aplicación (abstracción de persistencia).</param>
    public CreateProductHandler(IApplicationDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken ct)
    {
        // Regla: la categoría debe existir
        var categoryExists = await _db.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.CategoryId, ct);

        if (!categoryExists)
            throw new KeyNotFoundException("La categoría indicada no existe.");

        // Crear entidad de dominio (simple, sin EF attributes)
        var entity = new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Price = request.Price,
            Stock = request.Stock,
            CategoryId = request.CategoryId,
            IsActive = true
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);

        return entity.Id;
    }
}