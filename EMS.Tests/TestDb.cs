using EMS.Data;
using EMS.Models;
using EMS.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EMS.Tests;

// A fresh in-memory SQLite database per test. It is relational, so unique indexes and
// foreign keys are enforced just like on SQL Server.
public sealed class TestDb : IDisposable
{
    public const int Finance = 1, IT = 2, Restaurants = 3, Retail = 4, RealEstate = 5;

    private readonly SqliteConnection _connection;
    private readonly List<AppDbContext> _contexts = new();

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:;Foreign Keys=True");
        _connection.Open();
        Context = NewContext();
        Context.Database.EnsureCreated();
    }

    public AppDbContext Context { get; }

    // Each service gets its own context, like each web request does.
    public AppDbContext NewContext()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _contexts.Add(context);
        return context;
    }

    public MatchingService Matching() => new(NewContext());

    public SlotService Slots() => new(NewContext());

    public static TimeOnly T(int hour, int minute = 0) => new(hour, minute);

    public Room AddRoom(string hotelName, string roomName, params TimeOnly[] slotStarts)
    {
        var hotel = Context.Hotels.Local.FirstOrDefault(h => h.Name == hotelName)
                    ?? Context.Hotels.Add(new Hotel { Name = hotelName }).Entity;

        var room = new Room { Name = roomName, Hotel = hotel };
        foreach (var start in slotStarts)
        {
            room.RoomSlots.Add(new RoomSlot { StartTime = start, EndTime = start.Add(TimeRange.MeetingLength) });
        }

        Context.Rooms.Add(room);
        Context.SaveChanges();
        return room;
    }

    public static int SlotId(Room room, TimeOnly start) => room.RoomSlots.Single(s => s.StartTime == start).Id;

    public Investor AddInvestor(string name, params (int SectorId, TimeOnly From, TimeOnly To)[] windows)
    {
        var investor = new Investor { Name = name, Mobile = "01000000000" };
        foreach (var w in windows)
        {
            investor.Interests.Add(new InvestorInterest { SectorId = w.SectorId, StartTime = w.From, EndTime = w.To });
        }

        Context.Investors.Add(investor);
        Context.SaveChanges();
        return investor;
    }

    public Presenter AddPresenter(string name, params (int SectorId, TimeOnly From, TimeOnly To)[] windows)
    {
        var presenter = new Presenter { Name = name, Mobile = "01100000000" };
        foreach (var w in windows)
        {
            presenter.Interests.Add(new PresenterInterest { SectorId = w.SectorId, StartTime = w.From, EndTime = w.To });
        }

        Context.Presenters.Add(presenter);
        Context.SaveChanges();
        return presenter;
    }

    public void Dispose()
    {
        foreach (var context in _contexts)
        {
            context.Dispose();
        }
        _connection.Dispose();
    }
}
