using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Presenter
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, Phone, StringLength(20)]
    public string Mobile { get; set; } = string.Empty;

    public List<PresenterInterest> Interests { get; set; } = new();
}
