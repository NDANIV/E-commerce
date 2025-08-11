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
        var client = _factory.CreateClient();

        // 1) Registro y login (Customer)
        var email = $"user{Guid.NewGuid():N}@test.local";
        var password = "Secret123!";
        var reg = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        reg.StatusCode.Should().Be(HttpStatusCode.OK);

        var login = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // 2) Seed de producto en la BD (directo por contexto, para no requerir rol Admin)
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var cat = new Category { Id = Guid.NewGuid(), Name = "Test" };
            var prod = new Product { Id = Guid.NewGuid(), Name = "P1", Price = 25m, Stock = 10, CategoryId = cat.Id, IsActive = true };
            db.Categories.Add(cat);
            db.Products.Add(prod);
            await db.SaveChangesAsync();

            // 3) Agregar al carrito
            var add = await client.PostAsJsonAsync("/api/cart/items", new { productId = prod.Id, quantity = 2 });
            add.EnsureSuccessStatusCode();

            // 4) Checkout
            var co = await client.PostAsync("/api/orders/checkout", null);
            co.EnsureSuccessStatusCode();

            // 5) Mis pedidos
            var my = await client.GetAsync("/api/orders/my");
            my.EnsureSuccessStatusCode();
            var orders = await my.Content.ReadFromJsonAsync<List<ECommerce.Domain.Entities.Order>>();
            orders!.Should().NotBeNull().And.HaveCount(1);
            orders![0].Items.Should().ContainSingle(i => i.ProductId == prod.Id && i.Quantity == 2);
        }
    }
}

/// <summary>DTO de respuesta de login (igual que en tu WebApi).</summary>
public sealed record LoginResponse(string Token);
