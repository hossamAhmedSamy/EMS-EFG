namespace EMS.Models;

public class Investor
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;

    public List<InvestorInterest> Interests { get; set; } = new();
}
