namespace EMS.Models;

public class InvestorInterest
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int InvestorId { get; set; }
    public Investor? Investor { get; set; }

    public int SectorId { get; set; }
    public Sector? Sector { get; set; }
}
