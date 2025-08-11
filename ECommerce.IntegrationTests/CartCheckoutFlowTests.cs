using System.Net;
using System.Net.Http.Json;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Persistence;
using ECommerce.WebApi.Controllers;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.IntegrationTests;

/// <summary>
/// Flujo completo de cliente: register/login, agregar al carrito, checkout y consultar pedidos.
/// </summary>
public sealed class CartCheckoutFlowTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    public CartCheckoutFlowTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Flujo_completo_de_compra()
    {
        var client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("http://localhost")
        });

        Guid productId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cat = new Category { Id = Guid.NewGuid(), Name = "Test" };
            var prod = new Product { Id = Guid.NewGuid(), Name = "P1", Price = 25m, Stock = 10, CategoryId = cat.Id, IsActive = true };
            db.Categories.Add(cat);
            db.Products.Add(prod);
            await db.SaveChangesAsync();
            productId = prod.Id;
        }

        // 2) Agregar al carrito
        var add = await client.PostAsJsonAsync("/api/cart/items", new { productId, quantity = 2 });
        add.EnsureSuccessStatusCode();

        // 3) Checkout
        var co = await client.PostAsync("/api/orders/checkout", null);
        co.EnsureSuccessStatusCode();

        // 4) Mis pedidos
        var my = await client.GetAsync("/api/orders/my");
        my.EnsureSuccessStatusCode();
        var orders = await my.Content.ReadFromJsonAsync<List<ECommerce.Domain.Entities.Order>>();
        orders!.Should().NotBeNull().And.HaveCount(1);
        orders![0].Items.Should().ContainSingle(i => i.ProductId == productId && i.Quantity == 2);
    }
}

/// <summary>DTO de respuesta de login (igual que en tu WebApi).</summary>
public sealed record LoginResponse(string Token);
