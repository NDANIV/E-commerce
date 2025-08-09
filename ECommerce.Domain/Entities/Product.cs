namespace ECommerce.Domain.Entities;

/// <summary>
/// Representa un producto en el sistema de comercio electrónico.
/// Esta entidad contiene toda la información necesaria para gestionar
/// los productos disponibles en el catálogo.
/// </summary>
public class Product
{
    /// <summary>
    /// Identificador único del producto.
    /// Se genera automáticamente al crear un nuevo producto.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del producto. Este campo es obligatorio y se muestra
    /// en el catálogo y en los detalles del producto.
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Descripción detallada del producto. Este campo es opcional
    /// y proporciona información adicional sobre las características del producto.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Precio del producto en la moneda del sistema.
    /// Debe ser un valor positivo mayor a cero.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cantidad disponible en inventario.
    /// Un valor de 0 indica que el producto está agotado.
    /// </summary>
    public int Stock { get; set; }

    /// <summary>
    /// Identificador de la categoría a la que pertenece el producto.
    /// Representa la clave foránea hacia la entidad Category.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Propiedad de navegación hacia la categoría del producto.
    /// Permite acceder a los datos de la categoría asociada.
    /// </summary>
    public Category Category { get; set; } = default!;

    /// <summary>
    /// Indica si el producto está activo en el sistema.
    /// Los productos inactivos no se muestran en el catálogo público
    /// pero se mantienen en la base de datos para propósitos históricos.
    /// Por defecto es true (activo).
    /// </summary>
    public bool IsActive { get; set; } = true;
}