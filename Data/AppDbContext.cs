using EMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomSlot> RoomSlots => Set<RoomSlot>();
    public DbSet<Investor> Investors => Set<Investor>();
    public DbSet<Presenter> Presenters => Set<Presenter>();
    public DbSet<InvestorInterest> InvestorInterests => Set<InvestorInterest>();
    public DbSet<PresenterInterest> PresenterInterests => Set<PresenterInterest>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Sector>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(
                new Sector { Id = 1, Name = "Finance" },
                new Sector { Id = 2, Name = "IT" },
                new Sector { Id = 3, Name = "Restaurants" },
                new Sector { Id = 4, Name = "Retail" },
                new Sector { Id = 5, Name = "Real Estate" });
        });

        b.Entity<Hotel>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(200);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<Room>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasIndex(x => new { x.HotelId, x.Name }).IsUnique();   // no two "Room 1" in the same hotel
        });

        b.Entity<RoomSlot>(e =>
        {
            e.HasIndex(x => new { x.RoomId, x.StartTime }).IsUnique(); // no duplicate slot in a room
            e.ToTable(t => t.HasCheckConstraint("CK_RoomSlot_Time", "[EndTime] > [StartTime]"));
        });

        b.Entity<Investor>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Mobile).HasMaxLength(20);
        });

        b.Entity<Presenter>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.Property(x => x.Mobile).HasMaxLength(20);
        });

        b.Entity<InvestorInterest>(e =>
        {
            e.HasOne(x => x.Sector).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_InvestorInterest_Time", "[ToTime] > [FromTime]"));
        });

        b.Entity<PresenterInterest>(e =>
        {
            e.HasOne(x => x.Sector).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.ToTable(t => t.HasCheckConstraint("CK_PresenterInterest_Time", "[ToTime] > [FromTime]"));
        });

        b.Entity<Reservation>(e =>
        {
            // Deleting a person/room/sector must never silently wipe confirmed meetings.
            e.HasOne(x => x.Investor).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Presenter).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Sector).WithMany().OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.RoomSlot).WithOne(s => s.Reservation)
                .HasForeignKey<Reservation>(x => x.RoomSlotId).OnDelete(DeleteBehavior.Restrict);

            // Safety net behind the service's overlap check: one meeting per slot,
            // and nobody starts two meetings at the same time.
            e.HasIndex(x => x.RoomSlotId).IsUnique();
            e.HasIndex(x => new { x.InvestorId, x.StartTime }).IsUnique();
            e.HasIndex(x => new { x.PresenterId, x.StartTime }).IsUnique();
        });
    }
}
