namespace ECommerce.Domain.Entities;

public class Category
{
    /// <summary>
    /// Representa una categoria en el sistema de comercio electrónico.
    /// Esta entidad contiene toda la información necesaria para gestionar
    /// Las categorias disponibles.
    /// </summary>
    
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador único del producto.
    /// Se genera automáticamente al crear un nuevo producto.
    /// </summary>
    public string Name { get; set; } = default!;
    /// <summary>
    /// Descripción detallada de la categoria. Este campo es opcional.
    /// </summary>
    
    // Navegación, útil para pruebas
    public ICollection<Product> Products { get; set; } = new List<Product>();
}