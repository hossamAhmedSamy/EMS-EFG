using EMS.Models;
using EMS.Services;
using Microsoft.EntityFrameworkCore;
using static EMS.Tests.TestDb;

namespace EMS.Tests;

public class MatchingServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public void IsAvailable_needs_a_containing_window_and_no_overlapping_booking()
    {
        var windows = new[] { new TimeRange(T(13), T(16)) };
        var booked = new[] { new TimeRange(T(14), T(15)) };

        Assert.True(MatchingService.IsAvailable(new TimeRange(T(13), T(14)), windows, booked));
        Assert.False(MatchingService.IsAvailable(new TimeRange(T(14), T(15)), windows, booked));  // already booked
        Assert.False(MatchingService.IsAvailable(new TimeRange(T(14, 30), T(15, 30)), windows, booked)); // overlaps booking
        Assert.False(MatchingService.IsAvailable(new TimeRange(T(16), T(17)), windows, booked));  // outside window
    }

    [Fact]
    public async Task No_match_when_the_only_shared_hour_has_no_room_slot()
    {
        // Presenter free 2-3 and 4-6, investor free 1-2 and 5-6: the only hour both are free is 5-6.
        // The room is open 2-5, so there is no slot at 5-6.
        var investor = _db.AddInvestor("CIB", (Restaurants, T(13), T(14)), (Restaurants, T(17), T(18)));
        _db.AddPresenter("Oltob", (Restaurants, T(14), T(15)), (Restaurants, T(16), T(18)));
        _db.AddRoom("Hilton", "Room 1", T(14), T(15), T(16));

        var options = await _db.Matching().GetTimeOptionsAsync(investor.Id, Restaurants);

        Assert.Empty(options);
    }

    [Fact]
    public async Task Matches_at_5_PM_when_the_room_is_open_until_6()
    {
        var investor = _db.AddInvestor("CIB", (Restaurants, T(13), T(14)), (Restaurants, T(17), T(18)));
        _db.AddPresenter("Oltob", (Restaurants, T(14), T(15)), (Restaurants, T(16), T(18)));
        _db.AddRoom("Hilton", "Room 1", T(14), T(15), T(16), T(17));

        var options = await _db.Matching().GetTimeOptionsAsync(investor.Id, Restaurants);

        Assert.Equal(new[] { new TimeRange(T(17), T(18)) }, options);
    }

    [Fact]
    public async Task Task_example_Oltob_and_CIB_meet_only_from_1_PM()
    {
        var cib = _db.AddInvestor("CIB",
            (Finance, T(13), T(16)), (IT, T(13), T(16)), (Restaurants, T(13), T(16)), (RealEstate, T(13), T(16)));
        var oltob = _db.AddPresenter("Oltob", (Restaurants, T(8), T(16)));
        _db.AddRoom("Hilton", "Room 1", T(13), T(14), T(15));
        _db.AddRoom("Hilton", "Room 2", T(13), T(15));
        _db.AddRoom("Hilton", "Room 3", T(9), T(10)); // morning: Oltob is free, CIB is not

        var matching = _db.Matching();
        var options = await matching.GetTimeOptionsAsync(cib.Id, Restaurants);

        Assert.Equal(new[] { T(13), T(14), T(15) }, options.Select(o => o.Start));
        Assert.Equal(new[] { "Room 1", "Room 2" },
            (await matching.GetFreeSlotsAsync(new TimeRange(T(13), T(14)))).Select(s => s.Room!.Name));
        Assert.Equal(new[] { oltob.Id },
            (await matching.GetAvailablePresentersAsync(Restaurants, new TimeRange(T(13), T(14)))).Select(p => p.Id));

        // CIB is also in Finance, but no presenter is, so nothing matches there.
        Assert.Empty(await matching.GetTimeOptionsAsync(cib.Id, Finance));
    }

    [Fact]
    public async Task Booking_makes_the_room_investor_and_presenter_busy()
    {
        var cib = _db.AddInvestor("CIB", (Restaurants, T(13), T(16)));
        var nbe = _db.AddInvestor("NBE", (Restaurants, T(13), T(16)));
        var oltob = _db.AddPresenter("Oltob", (Restaurants, T(8), T(16)));
        var koshary = _db.AddPresenter("Koshary", (Restaurants, T(13), T(16)));
        var room1 = _db.AddRoom("Hilton", "Room 1", T(13), T(14));
        var room2 = _db.AddRoom("Hilton", "Room 2", T(13));

        Assert.Null(await _db.Matching().BookAsync(cib.Id, oltob.Id, Restaurants, SlotId(room1, T(13))));

        // Same room slot again.
        Assert.Contains("room is already booked",
            await _db.Matching().BookAsync(nbe.Id, koshary.Id, Restaurants, SlotId(room1, T(13))));
        // CIB is already in a meeting at 1 PM, even in a different room.
        Assert.Contains("investor is not free",
            await _db.Matching().BookAsync(cib.Id, koshary.Id, Restaurants, SlotId(room2, T(13))));
        // Oltob is already in a meeting at 1 PM.
        Assert.Contains("presenter is not free",
            await _db.Matching().BookAsync(nbe.Id, oltob.Id, Restaurants, SlotId(room2, T(13))));
        // Two free people in the other free room is fine.
        Assert.Null(await _db.Matching().BookAsync(nbe.Id, koshary.Id, Restaurants, SlotId(room2, T(13))));

        // CIB can no longer be offered 1 PM.
        var options = await _db.Matching().GetTimeOptionsAsync(cib.Id, Restaurants);
        Assert.Equal(new[] { T(14) }, options.Select(o => o.Start));
    }

    [Fact]
    public async Task A_booking_blocks_overlapping_times_not_just_the_same_time()
    {
        var cib = _db.AddInvestor("CIB", (Restaurants, T(13), T(16)));
        var oltob = _db.AddPresenter("Oltob", (Restaurants, T(13), T(16)));
        _db.AddPresenter("Koshary", (Restaurants, T(13), T(16)));
        var room1 = _db.AddRoom("Hilton", "Room 1", T(13));
        _db.AddRoom("Marriott", "Room A", T(13, 30), T(14, 30));

        Assert.Null(await _db.Matching().BookAsync(cib.Id, oltob.Id, Restaurants, SlotId(room1, T(13))));

        // 1:30-2:30 overlaps CIB's 1-2 meeting; 2:30-3:30 does not.
        var options = await _db.Matching().GetTimeOptionsAsync(cib.Id, Restaurants);
        Assert.Equal(new[] { T(14, 30) }, options.Select(o => o.Start));
    }

    [Fact]
    public async Task Booking_is_refused_when_the_presenter_is_not_in_that_sector()
    {
        var cib = _db.AddInvestor("CIB", (Restaurants, T(13), T(16)), (IT, T(13), T(16)));
        var techCo = _db.AddPresenter("TechCo", (IT, T(13), T(16)));
        var room = _db.AddRoom("Hilton", "Room 1", T(13));

        Assert.Contains("presenter is not free",
            await _db.Matching().BookAsync(cib.Id, techCo.Id, Restaurants, SlotId(room, T(13))));
        Assert.Null(await _db.Matching().BookAsync(cib.Id, techCo.Id, IT, SlotId(room, T(13))));
    }

    [Fact]
    public async Task Database_refuses_two_reservations_on_the_same_slot()
    {
        var cib = _db.AddInvestor("CIB", (Restaurants, T(13), T(16)));
        var nbe = _db.AddInvestor("NBE", (Restaurants, T(13), T(16)));
        var oltob = _db.AddPresenter("Oltob", (Restaurants, T(13), T(16)));
        var room = _db.AddRoom("Hilton", "Room 1", T(13));
        var slotId = SlotId(room, T(13));

        // Bypass the service on purpose: this checks the unique index backstop on its own.
        var context = _db.NewContext();
        context.Reservations.Add(NewReservation(cib.Id, oltob.Id, slotId));
        context.Reservations.Add(NewReservation(nbe.Id, oltob.Id, slotId));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task Database_refuses_deleting_a_person_who_has_a_reservation()
    {
        var cib = _db.AddInvestor("CIB", (Restaurants, T(13), T(16)));
        var oltob = _db.AddPresenter("Oltob", (Restaurants, T(13), T(16)));
        var room = _db.AddRoom("Hilton", "Room 1", T(13));
        Assert.Null(await _db.Matching().BookAsync(cib.Id, oltob.Id, Restaurants, SlotId(room, T(13))));

        var context = _db.NewContext();
        context.Investors.Remove(await context.Investors.SingleAsync(i => i.Id == cib.Id));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static Reservation NewReservation(int investorId, int presenterId, int slotId) => new()
    {
        InvestorId = investorId,
        PresenterId = presenterId,
        SectorId = Restaurants,
        RoomSlotId = slotId,
        StartTime = T(13),
        EndTime = T(14)
    };
}
