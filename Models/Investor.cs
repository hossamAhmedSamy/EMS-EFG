using System.ComponentModel.DataAnnotations;

namespace EMS.Models;

public class Investor
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mobile is required.")]
    [Phone(ErrorMessage = "Enter a valid mobile number.")]
    [StringLength(20, ErrorMessage = "Mobile can be at most 20 characters.")]
    public string Mobile { get; set; } = string.Empty;

    public List<InvestorInterest> Interests { get; set; } = new();
}
