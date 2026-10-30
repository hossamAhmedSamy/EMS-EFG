namespace EMS.Models;

public class Reservation
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int RoomSlotId { get; set; }
    public RoomSlot? RoomSlot { get; set; }

    public int InvestorId { get; set; }
    public Investor? Investor { get; set; }

    public int PresenterId { get; set; }
    public Presenter? Presenter { get; set; }
}
