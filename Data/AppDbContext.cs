using EcomApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcomApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // كل إيميل لازم يكون Unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // العلاقة بين OrderItem و Product
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);
    }
}
