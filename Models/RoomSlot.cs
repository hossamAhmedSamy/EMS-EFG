namespace EMS.Models;

public class RoomSlot
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }
}
