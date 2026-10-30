using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Hotel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Address can be at most 200 characters.")]
    public string? Address { get; set; }

    public List<Room> Rooms { get; set; } = new();
}
