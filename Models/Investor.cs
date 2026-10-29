namespace EMS.Models;

public class Investor
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Mobile { get; set; } = "";

    public List<InvestorInterest> Interests { get; set; } = [];
}
