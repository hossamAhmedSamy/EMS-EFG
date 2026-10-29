namespace EMS.Models;

// One sector a presenter cares about, with the time window for that sector.
public class PresenterInterest
{
    public int Id { get; set; }
    public int PresenterId { get; set; }
    public int SectorId { get; set; }
    public TimeOnly FromTime { get; set; }
    public TimeOnly ToTime { get; set; }

    public Presenter Presenter { get; set; } = null!;
    public Sector Sector { get; set; } = null!;
}
