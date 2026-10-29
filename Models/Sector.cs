namespace EMS.Models;

// A topic both sides can be interested in (Finance, IT, ...).
public class Sector
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
