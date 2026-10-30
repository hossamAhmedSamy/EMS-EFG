using System.Data;
using EMS.Data;
using EMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.Services;

public class MatchingService
{
    private readonly AppDbContext _context;

    public MatchingService(AppDbContext context)
    {
        _context = context;
    }

    // Free for a meeting = one of their windows fully contains it, and none of their bookings overlaps it.
    public static bool IsAvailable(TimeRange meeting, IEnumerable<TimeRange> windows, IEnumerable<TimeRange> booked)
        => windows.Any(w => w.Contains(meeting)) && !booked.Any(b => b.Overlaps(meeting));

    public static bool AllCovered(IEnumerable<TimeRange> booked, IEnumerable<TimeRange> windows)
        => booked.All(b => windows.Any(w => w.Contains(b)));

    // Meeting times the investor can book in this sector: a free room slot exists at that time,
    // the investor is free then, and at least one presenter in the sector is free then.
    public async Task<List<TimeRange>> GetTimeOptionsAsync(int investorId, int sectorId)
    {
        var investorWindows = await InvestorWindowsAsync(investorId, sectorId);
        if (investorWindows.Count == 0)
            return new List<TimeRange>();

        var investorBooked = await InvestorBookedAsync(investorId);
        var presenters = await PresentersInSectorAsync(sectorId);

        var freeSlotTimes = await _context.RoomSlots
            .Where(s => !_context.Reservations.Any(r => r.RoomSlotId == s.Id))
            .Select(s => new { s.StartTime, s.EndTime })
            .Distinct()
            .ToListAsync();

        return freeSlotTimes
            .Select(s => new TimeRange(s.StartTime, s.EndTime))
            .Where(t => IsAvailable(t, investorWindows, investorBooked)
                        && presenters.Any(p => IsAvailable(t, p.Windows, p.Booked)))
            .OrderBy(t => t.Start)
            .ToList();
    }

    public async Task<List<Presenter>> GetAvailablePresentersAsync(int sectorId, TimeRange meeting)
    {
        var presenters = await PresentersInSectorAsync(sectorId);
        return presenters
            .Where(p => IsAvailable(meeting, p.Windows, p.Booked))
            .Select(p => p.Presenter)
            .ToList();
    }

    public async Task<List<RoomSlot>> GetFreeSlotsAsync(TimeRange meeting)
    {
        return await _context.RoomSlots
            .Include(s => s.Room!)
                .ThenInclude(r => r.Hotel)
            .Where(s => s.StartTime == meeting.Start
                        && s.EndTime == meeting.End
                        && !_context.Reservations.Any(r => r.RoomSlotId == s.Id))
            .OrderBy(s => s.Room!.Hotel!.Name)
                .ThenBy(s => s.Room!.Name)
            .ToListAsync();
    }

    // Re-checks every rule at save time: the page the user saw may be stale by the time they confirm.
    // Returns null on success, otherwise the reason it was refused.
    public async Task<string?> BookAsync(int investorId, int presenterId, int sectorId, int roomSlotId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var slot = await _context.RoomSlots.FirstOrDefaultAsync(s => s.Id == roomSlotId);
        if (slot == null)
            return "That room slot no longer exists.";

        var meeting = new TimeRange(slot.StartTime, slot.EndTime);

        if (await _context.Reservations.AnyAsync(r => r.RoomSlotId == roomSlotId))
            return "That room is already booked at this time.";

        if (!IsAvailable(meeting, await InvestorWindowsAsync(investorId, sectorId), await InvestorBookedAsync(investorId)))
            return "The investor is not free in this sector at that time.";

        if (!IsAvailable(meeting, await PresenterWindowsAsync(presenterId, sectorId), await PresenterBookedAsync(presenterId)))
            return "The presenter is not free in this sector at that time.";

        _context.Reservations.Add(new Reservation
        {
            RoomSlotId = slot.Id,
            InvestorId = investorId,
            PresenterId = presenterId,
            SectorId = sectorId,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime
        });

        try
        {
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return "Someone else just booked a conflicting meeting. Please choose again.";
        }

        return null;
    }

    private Task<List<TimeRange>> InvestorWindowsAsync(int investorId, int sectorId) =>
        _context.InvestorInterests
            .Where(x => x.InvestorId == investorId && x.SectorId == sectorId)
            .Select(x => new TimeRange(x.StartTime, x.EndTime))
            .ToListAsync();

    private Task<List<TimeRange>> InvestorBookedAsync(int investorId) =>
        _context.Reservations
            .Where(r => r.InvestorId == investorId)
            .Select(r => new TimeRange(r.StartTime, r.EndTime))
            .ToListAsync();

    private Task<List<TimeRange>> PresenterWindowsAsync(int presenterId, int sectorId) =>
        _context.PresenterInterests
            .Where(x => x.PresenterId == presenterId && x.SectorId == sectorId)
            .Select(x => new TimeRange(x.StartTime, x.EndTime))
            .ToListAsync();

    private Task<List<TimeRange>> PresenterBookedAsync(int presenterId) =>
        _context.Reservations
            .Where(r => r.PresenterId == presenterId)
            .Select(r => new TimeRange(r.StartTime, r.EndTime))
            .ToListAsync();

    private async Task<List<PresenterAvailability>> PresentersInSectorAsync(int sectorId)
    {
        var presenters = await _context.Presenters
            .Where(p => p.Interests.Any(x => x.SectorId == sectorId))
            .OrderBy(p => p.Name)
            .ToListAsync();
        var ids = presenters.Select(p => p.Id).ToList();

        var windows = await _context.PresenterInterests
            .Where(x => x.SectorId == sectorId)
            .Select(x => new { x.PresenterId, x.StartTime, x.EndTime })
            .ToListAsync();

        var booked = await _context.Reservations
            .Where(r => ids.Contains(r.PresenterId))
            .Select(r => new { r.PresenterId, r.StartTime, r.EndTime })
            .ToListAsync();

        return presenters
            .Select(p => new PresenterAvailability(
                p,
                windows.Where(w => w.PresenterId == p.Id).Select(w => new TimeRange(w.StartTime, w.EndTime)).ToList(),
                booked.Where(b => b.PresenterId == p.Id).Select(b => new TimeRange(b.StartTime, b.EndTime)).ToList()))
            .ToList();
    }

    private record PresenterAvailability(Presenter Presenter, List<TimeRange> Windows, List<TimeRange> Booked);
}
