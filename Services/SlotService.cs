using EMS.Data;
using EMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EMS.Services;

public class SlotService
{
    private readonly AppDbContext _context;

    public SlotService(AppDbContext context)
    {
        _context = context;
    }

    // A leftover shorter than one hour at the end of the range is dropped.
    public static List<TimeRange> Generate(TimeOnly from, TimeOnly to)
    {
        var slots = new List<TimeRange>();
        var start = from;
        while (true)
        {
            var end = start.Add(TimeRange.MeetingLength, out var wrappedDays);
            if (wrappedDays > 0 || end > to)
                break;
            slots.Add(new TimeRange(start, end));
            start = end;
        }
        return slots;
    }

    public async Task<(int Added, int Skipped)> AddSlotsAsync(int roomId, TimeOnly from, TimeOnly to)
    {
        var existing = await _context.RoomSlots
            .Where(s => s.RoomId == roomId)
            .Select(s => new TimeRange(s.StartTime, s.EndTime))
            .ToListAsync();
        int added = 0, skipped = 0;

        foreach (var slot in Generate(from, to))
        {
            if (existing.Any(e => e.Overlaps(slot)))
            {
                skipped++;
                continue;
            }

            _context.RoomSlots.Add(new RoomSlot { RoomId = roomId, StartTime = slot.Start, EndTime = slot.End });
            existing.Add(slot);
            added++;
        }

        await _context.SaveChangesAsync();
        return (added, skipped);
    }

    public async Task<bool> RemoveSlotAsync(int slotId)
    {
        if (await _context.Reservations.AnyAsync(r => r.RoomSlotId == slotId))
            return false;

        await _context.RoomSlots.Where(s => s.Id == slotId).ExecuteDeleteAsync();
        return true;
    }
}
