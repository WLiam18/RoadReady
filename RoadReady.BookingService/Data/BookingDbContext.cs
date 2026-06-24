using Microsoft.EntityFrameworkCore;
using RoadReady.BookingService.Models;

namespace RoadReady.BookingService.Data;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Payment> Payments { get; set; }
    public DbSet<VehicleInspection> VehicleInspections => Set<VehicleInspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Booking>()
            .Property(x => x.Status)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleInspection>()
            .Property(x => x.Type)
            .HasConversion<string>();

        modelBuilder.Entity<VehicleInspection>()
            .HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}