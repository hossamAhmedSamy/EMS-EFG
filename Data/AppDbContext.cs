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

        modelBuilder.Entity<Reservation>()
            .HasIndex(r => r.RoomSlotId)
            .IsUnique();
    }
}
