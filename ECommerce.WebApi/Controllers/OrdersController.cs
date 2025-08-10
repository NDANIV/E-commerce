using ECommerce.Application.Checkout.Commands;
using ECommerce.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities;

namespace ECommerce.WebApi.Controllers;

/// <summary>Endpoints para realizar checkout y consultar pedidos.</summary>
[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _db;

    public OrdersController(IMediator mediator, IApplicationDbContext db)
    {
        _mediator = mediator;
        _db = db;
    }

    /// <summary>Realiza el checkout del carrito del usuario autenticado.</summary>
    [HttpPost("checkout")]
    [Authorize]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> Checkout()
    {
        var userId = User.GetUserId();
        var orderId = await _mediator.Send(new CheckoutCommand(userId));
        return Ok(orderId);
    }

    /// <summary>Lista pedidos del usuario autenticado.</summary>
    [HttpGet("my")]
    [Authorize]
    [ProducesResponseType(typeof(List<Order>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MyOrders()
    {
        var userId = User.GetUserId();
        var orders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders);
    }

    /// <summary>Detalle de pedido del usuario autenticado.</summary>
    [HttpGet("{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(Order), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid orderId)
    {
        var userId = User.GetUserId();
        var order = await _db.Orders.Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Admin: actualiza el estado de un pedido.</summary>
    [HttpPatch("{orderId:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus([FromRoute] Guid orderId, [FromBody] UpdateOrderStatusRequest req)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return NotFound();

        order.Status = req.Status;
        await _db.SaveChangesAsync();
        return Ok();
    }
}

/// <summary>DTO para cambiar el estado de un pedido.</summary>
public sealed class UpdateOrderStatusRequest
{
    /// <summary>Nuevo estado del pedido.</summary>
    public OrderStatus Status { get; set; }
}
