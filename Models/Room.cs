namespace EMS.Models;

// A conference room inside a hotel.
public class Room
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public string Name { get; set; } = "";

    public Hotel Hotel { get; set; } = null!;
    public List<RoomSlot> Slots { get; set; } = [];
}
