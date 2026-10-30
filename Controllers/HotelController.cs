using EMS.Data;
using EMS.Models;
using EMS.Services;
using EMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers;

public class HotelController : Controller
{
    private readonly AppDbContext _context;
    private readonly SlotService _slotService;

    public HotelController(AppDbContext context, SlotService slotService)
    {
        _context = context;
        _slotService = slotService;
    }

    public async Task<IActionResult> Index()
    {
        var hotels = await _context.Hotels
            .Include(h => h.Rooms)
            .OrderBy(h => h.Name)
            .ToListAsync();
        return View(hotels);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Hotel hotel)
    {
        if (await _context.Hotels.AnyAsync(h => h.Name == hotel.Name))
        {
            ModelState.AddModelError(nameof(Hotel.Name), "A hotel with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(hotel);
        }

        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Manage), new { id = hotel.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }
        return View(hotel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Hotel input)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        if (await _context.Hotels.AnyAsync(h => h.Name == input.Name && h.Id != id))
        {
            ModelState.AddModelError(nameof(Hotel.Name), "A hotel with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            input.Id = id;
            return View(input);
        }

        // Copy only the editable fields, so a tampered form can't change anything else.
        hotel.Name = input.Name;
        hotel.Address = input.Address;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Hotel updated.";
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var hotel = await _context.Hotels.FindAsync(id);
        if (hotel == null)
        {
            return NotFound();
        }

        if (await _context.Reservations.AnyAsync(r => r.RoomSlot!.Room!.HotelId == id))
        {
            TempData["Error"] = "This hotel has reservations. Cancel them before deleting the hotel.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        _context.Hotels.Remove(hotel);
        await _context.SaveChangesAsync();
        TempData["Message"] = $"Hotel \"{hotel.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Manage(int id)
    {
        var hotel = await _context.Hotels
            .Include(h => h.Rooms)
                .ThenInclude(r => r.RoomSlots)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (hotel == null)
        {
            return NotFound();
        }

        var reservedSlotIds = await _context.Reservations
            .Where(r => r.RoomSlot!.Room!.HotelId == id)
            .Select(r => r.RoomSlotId)
            .ToListAsync();

        return View(new HotelManageViewModel
        {
            Hotel = hotel,
            ReservedSlotIds = reservedSlotIds.ToHashSet()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoom(int hotelId, string? roomName)
    {
        if (!await _context.Hotels.AnyAsync(h => h.Id == hotelId))
        {
            return NotFound();
        }

        roomName = roomName?.Trim();
        var error = await ValidateRoomNameAsync(hotelId, roomName);

        if (error != null)
        {
            TempData["Error"] = error;
        }
        else
        {
            _context.Rooms.Add(new Room { HotelId = hotelId, Name = roomName! });
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Room \"{roomName}\" added.";
        }

        return RedirectToAction(nameof(Manage), new { id = hotelId });
    }

    [HttpGet]
    public async Task<IActionResult> EditRoom(int id)
    {
        var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);
        if (room == null)
        {
            return NotFound();
        }
        return View(room);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoom(int id, string? name)
    {
        var room = await _context.Rooms.Include(r => r.Hotel).FirstOrDefaultAsync(r => r.Id == id);
        if (room == null)
        {
            return NotFound();
        }

        name = name?.Trim();
        var error = await ValidateRoomNameAsync(room.HotelId, name, room.Id);
        if (error != null)
        {
            ModelState.AddModelError(nameof(Room.Name), error);
            return View(room);
        }

        room.Name = name!;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Room renamed.";
        return RedirectToAction(nameof(Manage), new { id = room.HotelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        if (await _context.Reservations.AnyAsync(r => r.RoomSlot!.RoomId == id))
        {
            TempData["Error"] = $"{room.Name} has reservations. Cancel them before deleting the room.";
        }
        else
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Room \"{room.Name}\" deleted.";
        }

        return RedirectToAction(nameof(Manage), new { id = room.HotelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSlots(int roomId, TimeOnly from, TimeOnly to)
    {
        var room = await _context.Rooms.FindAsync(roomId);
        if (room == null)
        {
            return NotFound();
        }

        var error = ModelState.IsValid
            ? TimeRange.Validate(from, to)
            : "Please enter both a start and an end time.";

        if (error != null)
        {
            TempData["Error"] = error;
        }
        else
        {
            var (added, skipped) = await _slotService.AddSlotsAsync(roomId, from, to);
            TempData["Message"] = skipped == 0
                ? $"Added {added} slot(s) to {room.Name}."
                : $"Added {added} slot(s) to {room.Name}; skipped {skipped} that overlap existing slots.";
        }

        return RedirectToAction(nameof(Manage), new { id = room.HotelId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveSlot(int slotId)
    {
        var slot = await _context.RoomSlots
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == slotId);
        if (slot == null)
        {
            return NotFound();
        }

        if (await _slotService.RemoveSlotAsync(slotId))
        {
            TempData["Message"] = "Slot removed.";
        }
        else
        {
            TempData["Error"] = "This slot is reserved and cannot be removed.";
        }

        return RedirectToAction(nameof(Manage), new { id = slot.Room!.HotelId });
    }

    private async Task<string?> ValidateRoomNameAsync(int hotelId, string? name, int excludeRoomId = 0)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 50)
            return "Room name is required (50 characters max).";

        if (await _context.Rooms.AnyAsync(r => r.HotelId == hotelId && r.Name == name && r.Id != excludeRoomId))
            return $"This hotel already has a room named \"{name}\".";

        return null;
    }
}
