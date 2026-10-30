namespace EMS.Models;

public class Presenter
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;

    public List<PresenterInterest> Interests { get; set; } = new();
}
