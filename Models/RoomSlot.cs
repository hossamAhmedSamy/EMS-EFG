namespace EMS.Models;

// A one-hour window when a room can host a meeting (e.g. 1:00 PM - 2:00 PM).
public class RoomSlot
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public Room Room { get; set; } = null!;
    public Reservation? Reservation { get; set; }
}
