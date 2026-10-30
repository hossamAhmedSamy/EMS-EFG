using EMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomSlot> RoomSlots => Set<RoomSlot>();
    public DbSet<Investor> Investors => Set<Investor>();
    public DbSet<Presenter> Presenters => Set<Presenter>();
    public DbSet<InvestorInterest> InvestorInterests => Set<InvestorInterest>();
    public DbSet<PresenterInterest> PresenterInterests => Set<PresenterInterest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>()
            .HasIndex(h => h.Name)
            .IsUnique();

        modelBuilder.Entity<Room>()
            .HasIndex(r => new { r.HotelId, r.Name })
            .IsUnique();

        modelBuilder.Entity<Sector>()
            .HasIndex(s => s.Name)
            .IsUnique();

        modelBuilder.Entity<Reservation>(reservation =>
        {
            reservation.HasIndex(r => r.RoomSlotId).IsUnique();

            // Restrict: the database refuses to delete a slot, person or sector that still has a reservation.
            reservation.HasOne(r => r.RoomSlot).WithMany().HasForeignKey(r => r.RoomSlotId).OnDelete(DeleteBehavior.Restrict);
            reservation.HasOne(r => r.Investor).WithMany().HasForeignKey(r => r.InvestorId).OnDelete(DeleteBehavior.Restrict);
            reservation.HasOne(r => r.Presenter).WithMany().HasForeignKey(r => r.PresenterId).OnDelete(DeleteBehavior.Restrict);
            reservation.HasOne(r => r.Sector).WithMany().HasForeignKey(r => r.SectorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Sector>().HasData(
            new Sector { Id = 1, Name = "Finance" },
            new Sector { Id = 2, Name = "IT" },
            new Sector { Id = 3, Name = "Restaurants" },
            new Sector { Id = 4, Name = "Retail" },
            new Sector { Id = 5, Name = "Real Estate" });
    }
}
