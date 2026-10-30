using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Hotel
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(200)]
    public string? Address { get; set; }

    public List<Room> Rooms { get; set; } = new();
}
