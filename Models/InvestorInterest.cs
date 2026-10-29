namespace EMS.Models;

// One sector an investor cares about, with the time window for that sector.
public class InvestorInterest
{
    public int Id { get; set; }
    public int InvestorId { get; set; }
    public int SectorId { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }

    public Investor Investor { get; set; } = null!;
    public Sector Sector { get; set; } = null!;
}
