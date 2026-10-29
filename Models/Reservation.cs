namespace EMS.Models;

// A confirmed meeting: investor + presenter + sector, held in one room slot.
// StartTime/EndTime are copied from the slot so "is this person busy?" checks
// don't need to join through RoomSlot.
public class Reservation
{
    public int Id { get; set; }
    public int InvestorId { get; set; }
    public int PresenterId { get; set; }
    public int SectorId { get; set; }
    public int RoomSlotId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Investor Investor { get; set; } = null!;
    public Presenter Presenter { get; set; } = null!;
    public Sector Sector { get; set; } = null!;
    public RoomSlot RoomSlot { get; set; } = null!;
}
