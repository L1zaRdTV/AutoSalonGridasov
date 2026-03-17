using AutoSalonGrida.Models;
using Microsoft.EntityFrameworkCore;

namespace AutoSalonGrida.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CarImage> CarImages => Set<CarImage>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        builder.Entity<Car>()
            .HasOne(c => c.Category)
            .WithMany(c => c.Cars)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<CarImage>()
            .HasOne(ci => ci.Car)
            .WithMany(c => c.Images)
            .HasForeignKey(ci => ci.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Cart>()
            .HasOne(c => c.User)
            .WithMany(u => u.Carts)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Cart>()
            .HasIndex(c => c.UserId)
            .IsUnique();

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Cart)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<CartItem>()
            .HasOne(ci => ci.Car)
            .WithMany()
            .HasForeignKey(ci => ci.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<OrderItem>()
            .HasOne(oi => oi.Car)
            .WithMany()
            .HasForeignKey(oi => oi.CarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "SUV" },
            new Category { Id = 2, Name = "Седан" },
            new Category { Id = 3, Name = "Хэтчбек" },
            new Category { Id = 4, Name = "Купе" },
            new Category { Id = 5, Name = "Электромобиль" },
            new Category { Id = 6, Name = "Гибрид" },
            new Category { Id = 7, Name = "Спорткар" },
            new Category { Id = 8, Name = "Люкс" }
        );
    }
}
