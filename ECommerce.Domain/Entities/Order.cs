namespace ECommerce.Domain.Entities;

/// <summary>
/// Representa un pedido realizado por un usuario en el sistema de comercio electrónico.
/// Un pedido contiene uno o más productos (OrderItem) y mantiene el estado
/// del proceso de compra desde la creación hasta la entrega.
/// </summary>
public class Order
{
    /// <summary>
    /// Identificador único del pedido.
    /// Se genera automáticamente al crear un nuevo pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del usuario que realizó el pedido.
    /// Representa la clave foránea hacia la entidad User.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Fecha y hora de creación del pedido en UTC.
    /// Se establece automáticamente al momento de crear el pedido.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Estado actual del pedido en el flujo de procesamiento.
    /// Por defecto se inicia como Pending (Pendiente).
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Colección de elementos (productos) incluidos en este pedido.
    /// Cada elemento contiene la información del producto, cantidad y precio
    /// al momento de realizar el pedido.
    /// </summary>
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    /// <summary>
    /// Total del pedido calculado automáticamente.
    /// Se obtiene sumando el precio unitario por la cantidad de cada elemento.
    /// Este valor se calcula en tiempo real basado en los items del pedido.
    /// </summary>
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}

/// <summary>
/// Representa un elemento individual dentro de un pedido.
/// Contiene la información específica de un producto en el pedido,
/// incluyendo cantidad y precio histórico al momento de la compra.
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Identificador único del elemento del pedido.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del pedido al que pertenece este elemento.
    /// Representa la clave foránea hacia la entidad Order.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Identificador del producto asociado a este elemento.
    /// Representa la clave foránea hacia la entidad Product.
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Cantidad del producto solicitada en este pedido.
    /// Debe ser un valor positivo mayor a cero.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Precio unitario del producto al momento de realizar el pedido.
    /// Este valor se guarda históricamente para mantener la integridad
    /// del pedido aunque el precio del producto cambie posteriormente.
    /// Esto es una práctica importante en sistemas de e-commerce.
    /// </summary>
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Estados posibles de un pedido durante su ciclo de vida.
/// Define el flujo de procesamiento desde la creación hasta la finalización.
/// </summary>
public enum OrderStatus 
{ 
    /// <summary>
    /// Pedido creado pero aún no pagado.
    /// Estado inicial de todos los pedidos.
    /// </summary>
    Pending, 
    
    /// <summary>
    /// Pedido pagado exitosamente.
    /// El pago ha sido procesado y confirmado.
    /// </summary>
    Paid, 
    
    /// <summary>
    /// Pedido enviado al cliente.
    /// Los productos han sido despachados para entrega.
    /// </summary>
    Shipped, 
    
    /// <summary>
    /// Pedido entregado al cliente.
    /// Estado final exitoso del proceso.
    /// </summary>
    Delivered, 
    
    /// <summary>
    /// Pedido cancelado.
    /// Puede ser cancelado por el cliente o por el sistema.
    /// </summary>
    Canceled 
}