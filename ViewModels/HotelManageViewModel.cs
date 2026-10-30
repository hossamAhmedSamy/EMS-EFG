using EMS.Models;

namespace EMS.ViewModels;

public class HotelManageViewModel
{
    public Hotel Hotel { get; set; } = null!;
    public HashSet<int> ReservedSlotIds { get; set; } = new();
}
