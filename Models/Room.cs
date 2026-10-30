using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Room name is required.")]
    [StringLength(50, ErrorMessage = "Room name can be at most 50 characters.")]
    public string Name { get; set; } = string.Empty;

    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }

    public List<RoomSlot> RoomSlots { get; set; } = new();
}
