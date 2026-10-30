using EMS.Models;

namespace EMS.ViewModels;

public class PresenterManageViewModel
{
    public Presenter Presenter { get; set; } = null!;
    public List<Sector> Sectors { get; set; } = new();
}
