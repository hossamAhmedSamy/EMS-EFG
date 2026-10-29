namespace EMS.Models;

public class Presenter
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Mobile { get; set; } = "";

    public List<PresenterInterest> Interests { get; set; } = [];
}
