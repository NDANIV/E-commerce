using ECommerce.Application.Cart.Commands;
using ECommerce.Application.Cart.Dtos;
using ECommerce.Application.Cart.Queries;
using ECommerce.WebApi.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.WebApi.Controllers;

/// <summary>Endpoints para administrar el carrito del usuario autenticado.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // todos requieren autenticación
public sealed class CartController : ControllerBase
{
    private readonly IMediator _mediator;
    public CartController(IMediator mediator) => _mediator = mediator;

    /// <summary>Obtiene el carrito del usuario. Lo crea si no existe.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get()
    {
        var userId = User.GetUserId();
        var cart = await _mediator.Send(new GetCartQuery(userId));
        return Ok(cart);
    }

    /// <summary>Agrega un producto al carrito (o incrementa su cantidad).</summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new AddToCartCommand(userId, request.ProductId, request.Quantity));
        return Ok();
    }

    /// <summary>Actualiza la cantidad de un ítem del carrito.</summary>
    [HttpPut("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateQty([FromBody] UpdateQtyRequest request)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new UpdateCartItemQtyCommand(userId, request.ProductId, request.Quantity));
        return Ok();
    }

    /// <summary>Elimina un producto del carrito.</summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveItem([FromRoute] Guid productId)
    {
        var userId = User.GetUserId();
        await _mediator.Send(new RemoveFromCartCommand(userId, productId));
        return Ok();
    }

    /// <summary>Limpia completamente el carrito.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Clear()
    {
        var userId = User.GetUserId();
        await _mediator.Send(new ClearCartCommand(userId));
        return Ok();
    }
}

/// <summary>DTO para agregar un ítem al carrito.</summary>
public sealed class AddItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

/// <summary>DTO para actualizar la cantidad de un ítem del carrito.</summary>
public sealed class UpdateQtyRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
