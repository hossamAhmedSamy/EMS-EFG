using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Sector
{
    public int Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;
}
