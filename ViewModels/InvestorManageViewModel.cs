using EMS.Models;

namespace EMS.ViewModels;

public class InvestorManageViewModel
{
    public Investor Investor { get; set; } = null!;
    public List<Sector> Sectors { get; set; } = new();
}
