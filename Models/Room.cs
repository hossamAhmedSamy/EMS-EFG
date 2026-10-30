using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Room
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public List<RoomSlot> RoomSlots { get; set; } = new();
}
