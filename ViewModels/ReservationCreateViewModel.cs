using EMS.Models;
using EMS.Services;

namespace EMS.ViewModels;

// Each step is filled only once the previous one has a valid choice.
public class ReservationCreateViewModel
{
    public List<Investor> Investors { get; set; } = new();
    public int? InvestorId { get; set; }

    public List<InvestorInterest> InvestorWindows { get; set; } = new();
    public List<Sector> Sectors { get; set; } = new();
    public int? SectorId { get; set; }

    public List<TimeRange> TimeOptions { get; set; } = new();
    public TimeRange? Meeting { get; set; }

    public List<Presenter> Presenters { get; set; } = new();
    public List<RoomSlot> Slots { get; set; } = new();
}
