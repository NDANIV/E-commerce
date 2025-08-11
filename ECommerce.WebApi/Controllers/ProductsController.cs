using ECommerce.Application.Products.Commands;
using ECommerce.Application.Products.Dtos;
using ECommerce.Application.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebApi.Controllers;

/// <summary>
/// Endpoints para gestionar el catálogo de productos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Crea una nueva instancia del controlador de productos.
    /// </summary>
    /// <param name="mediator">Mediador para ejecutar Queries/Commands (CQRS).</param>
    public ProductsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Lista productos activos con filtros opcionales.
    /// </summary>
    /// <param name="search">Texto a buscar en el nombre del producto (contiene).</param>
    /// <param name="categoryId">Id de la categoría a filtrar.</param>
    /// <returns>Lista de productos proyectados a <see cref="ProductDto"/>.</returns>
    /// <response code="200">Catálogo devuelto con éxito.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] string? search, [FromQuery] Guid? categoryId)
    {
        var items = await _mediator.Send(new GetProductsQuery(search, categoryId));
        return Ok(items);
    }

    /// <summary>
    /// Crea un nuevo producto. Requiere rol <c>Admin</c>.
    /// </summary>
    /// <param name="request">Datos del producto a crear.</param>
    /// <returns>Id del producto creado.</returns>
    /// <response code="200">Producto creado exitosamente.</response>
    /// <response code="400">Errores de validación.</response>
    /// <response code="401">No autenticado.</response>
    /// <response code="403">No autorizado (requiere rol Admin).</response>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var id = await _mediator.Send(new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.Stock,
            request.CategoryId
        ));
        return Ok(id);
    }
}

/// <summary>
/// Datos de entrada para crear un producto.
/// </summary>
public sealed class CreateProductRequest
{
    /// <summary>Nombre del producto (3-100 caracteres).</summary>
    public string Name { get; set; } = default!;

    /// <summary>Descripción (opcional, hasta 1000 carac.).</summary>
    public string? Description { get; set; }

    /// <summary>Precio de venta (>= 0).</summary>
    public decimal Price { get; set; }

    /// <summary>Cantidad inicial en inventario (>= 0).</summary>
    public int Stock { get; set; }

    /// <summary>Identificador de la categoría existente.</summary>
    public Guid CategoryId { get; set; }
}
