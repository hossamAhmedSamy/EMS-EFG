namespace EMS.Models;

public class PresenterInterest
{
    public int Id { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public int PresenterId { get; set; }
    public Presenter? Presenter { get; set; }

    public int SectorId { get; set; }
    public Sector? Sector { get; set; }
}
