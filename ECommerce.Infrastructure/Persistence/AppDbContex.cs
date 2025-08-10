using ECommerce.Application.Abstractions;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Persistence;

/// <summary>
/// DbContext principal de la aplicación. Hereda de
/// <see cref="IdentityDbContext{TUser, TRole, TKey}"/> para incluir
/// las tablas de ASP.NET Identity y, además, implementa
/// <see cref="IApplicationDbContext"/> para exponer colecciones de dominio
/// a la capa Application sin acoplarla a EF.
/// </summary>
public class AppDbContext
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    /// <summary>
    /// Crea una nueva instancia del contexto con las opciones configuradas por DI.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <inheritdoc/>
    public DbSet<Product> Products => Set<Product>();

    /// <inheritdoc/>
    public DbSet<Category> Categories => Set<Category>();

    /// <inheritdoc/>
    public DbSet<Order> Orders => Set<Order>();

    /// <inheritdoc/>
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    /// <summary>
    /// Configuración de mapeos y restricciones de modelos de dominio e Identity.
    /// </summary>
    /// <param name="modelBuilder">Constructor de modelos de EF Core.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ====== PRODUCT ======
        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(p => p.Id);

            b.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(p => p.Description)
                .HasMaxLength(1000);

            // Precisión para dinero: 18,2
            b.Property(p => p.Price)
                .HasPrecision(18, 2);

            b.Property(p => p.Stock)
                .IsRequired();

            b.Property(p => p.IsActive)
                .HasDefaultValue(true);

            b.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ====== CATEGORY ======
        modelBuilder.Entity<Category>(b =>
        {
            b.ToTable("Categories");
            b.HasKey(c => c.Id);

            b.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(80);
        });

        // ====== ORDER ======
        modelBuilder.Entity<Order>(b =>
        {
            b.ToTable("Orders");
            b.HasKey(o => o.Id);

            b.Property(o => o.CreatedAt)
                .IsRequired();

            b.Property(o => o.Status)
                .IsRequired();
        });

        // ====== ORDER ITEM ======
        modelBuilder.Entity<OrderItem>(b =>
        {
            b.ToTable("OrderItems");
            b.HasKey(oi => oi.Id);

            b.Property(oi => oi.Quantity)
                .IsRequired();

            b.Property(oi => oi.UnitPrice)
                .HasPrecision(18, 2);

            b.HasOne<Order>()
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== CART =====
    modelBuilder.Entity<Cart>(b =>
    {
        b.ToTable("Carts");
        b.HasKey(c => c.Id);
        b.Property(c => c.UserId).IsRequired();
        b.Property(c => c.UpdatedAt).IsRequired();

        b.HasMany(c => c.Items)
         .WithOne()
         .HasForeignKey(i => i.CartId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(c => c.UserId).IsUnique(); // un carrito por usuario
    });

    // ===== CART ITEM =====
    modelBuilder.Entity<CartItem>(b =>
    {
        b.ToTable("CartItems");
        b.HasKey(ci => ci.Id);

        b.Property(ci => ci.Quantity).IsRequired();
        b.Property(ci => ci.UnitPrice).HasPrecision(18, 2);

        b.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();
    });

        // Índices sugeridos (consultas más rápidas)
        modelBuilder.Entity<Product>().HasIndex(p => new { p.IsActive, p.CategoryId });
        modelBuilder.Entity<Product>().HasIndex(p => p.Name);
    }

    /// <inheritdoc/>
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
        => base.SaveChangesAsync(ct);
}