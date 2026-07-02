using Microsoft.EntityFrameworkCore;
using RoadReady.CarService.Models;

namespace RoadReady.CarService.Data;

public class CarDbContext : DbContext
{
    public CarDbContext(DbContextOptions<CarDbContext> options) : base(options) { }

    public DbSet<Car> Cars => Set<Car>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Brand>()
            .HasIndex(x => x.Name)
            .IsUnique();

        modelBuilder.Entity<Car>()
            .HasIndex(x => x.LicensePlate)
            .IsUnique();

        modelBuilder.Entity<Car>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<Car>()
            .Property(x => x.ImageUrls)
            .HasDefaultValue(string.Empty);

        modelBuilder.Entity<Car>()
            .HasOne(x => x.Brand)
            .WithMany(x => x.Cars)
            .HasForeignKey(x => x.BrandId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(x => x.Car)
            .WithMany(x => x.Reviews)
            .HasForeignKey(x => x.CarId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PromoCode>()
            .HasIndex(p => p.Code)
            .IsUnique();

        modelBuilder.Entity<PromoCode>()
            .Property(p => p.DiscountType)
            .HasConversion<string>();
    }
}