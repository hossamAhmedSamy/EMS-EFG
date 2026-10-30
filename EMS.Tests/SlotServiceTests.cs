using EMS.Services;
using Microsoft.EntityFrameworkCore;
using static EMS.Tests.TestDb;

namespace EMS.Tests;

public class SlotServiceTests
{
    [Fact]
    public void Generate_cuts_a_range_into_hours_and_drops_the_leftover()
    {
        var slots = SlotService.Generate(T(13), T(16, 30));

        Assert.Equal(new[]
        {
            new TimeRange(T(13), T(14)),
            new TimeRange(T(14), T(15)),
            new TimeRange(T(15), T(16)),
        }, slots);
    }

    [Fact]
    public void Generate_accepts_any_start_time()
    {
        var slots = SlotService.Generate(T(13, 30), T(15, 30));

        Assert.Equal(new[] { new TimeRange(T(13, 30), T(14, 30)), new TimeRange(T(14, 30), T(15, 30)) }, slots);
    }

    [Fact]
    public void Generate_never_wraps_past_midnight()
    {
        Assert.Empty(SlotService.Generate(T(23), T(23, 59)));
        Assert.Single(SlotService.Generate(T(22, 30), T(23, 59)));
    }

    [Fact]
    public async Task AddSlots_skips_slots_that_overlap_existing_ones_in_the_same_room()
    {
        using var db = new TestDb();
        var room = db.AddRoom("Hilton", "Room 1", T(13), T(14));

        var (added, skipped) = await db.Slots().AddSlotsAsync(room.Id, T(13, 30), T(16, 30));

        // Candidates 1:30, 2:30, 3:30: the first two overlap 1-2 / 2-3, only 3:30-4:30 is new.
        Assert.Equal(1, added);
        Assert.Equal(2, skipped);
        Assert.Equal(3, await db.NewContext().RoomSlots.CountAsync(s => s.RoomId == room.Id));
    }
}
