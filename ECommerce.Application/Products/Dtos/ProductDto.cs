namespace ECommerce.Application.Products.Dtos;

/// <summary>
/// Representación ligera y segura de un producto para respuestas de aplicación/API.
/// </summary>
/// <param name="Id">Id del producto.</param>
/// <param name="Name">Nombre del producto.</param>
/// <param name="Price">Precio de venta actual.</param>
/// <param name="Category">Nombre de la categoría.</param>
public record ProductDto(Guid Id, string Name, decimal Price, string Category);