using EMS.Data;
using EMS.Services;
using EMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers;

public class ReservationController : Controller
{
    private readonly AppDbContext _context;
    private readonly MatchingService _matching;

    public ReservationController(AppDbContext context, MatchingService matching)
    {
        _context = context;
        _matching = matching;
    }

    public async Task<IActionResult> Index()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Investor)
            .Include(r => r.Presenter)
            .Include(r => r.Sector)
            .Include(r => r.RoomSlot!)
                .ThenInclude(s => s.Room!)
                    .ThenInclude(room => room.Hotel)
            .OrderBy(r => r.StartTime)
            .ToListAsync();
        return View(reservations);
    }

    // Step-by-step page driven by the query string: investor -> sector -> time -> presenter + room.
    // A step's options are only loaded once the previous choice is valid.
    [HttpGet]
    public async Task<IActionResult> Create(int? investorId, int? sectorId, TimeOnly? start)
    {
        var vm = new ReservationCreateViewModel
        {
            Investors = await _context.Investors.OrderBy(i => i.Name).ToListAsync()
        };

        if (investorId == null || !vm.Investors.Any(i => i.Id == investorId))
            return View(vm);
        vm.InvestorId = investorId;

        vm.InvestorWindows = await _context.InvestorInterests
            .Include(x => x.Sector)
            .Where(x => x.InvestorId == investorId)
            .OrderBy(x => x.Sector!.Name)
                .ThenBy(x => x.StartTime)
            .ToListAsync();
        vm.Sectors = vm.InvestorWindows.Select(x => x.Sector!).DistinctBy(s => s.Id).ToList();

        if (sectorId == null || !vm.Sectors.Any(s => s.Id == sectorId))
            return View(vm);
        vm.SectorId = sectorId;

        vm.TimeOptions = await _matching.GetTimeOptionsAsync(investorId.Value, sectorId.Value);

        var meeting = vm.TimeOptions.FirstOrDefault(t => t.Start == start);
        if (start == null || meeting == default)
            return View(vm);
        vm.Meeting = meeting;

        vm.Presenters = await _matching.GetAvailablePresentersAsync(sectorId.Value, meeting);
        vm.Slots = await _matching.GetFreeSlotsAsync(meeting);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int investorId, int sectorId, TimeOnly start, int presenterId, int roomSlotId)
    {
        var error = ModelState.IsValid
            ? await _matching.BookAsync(investorId, presenterId, sectorId, roomSlotId)
            : "Please choose a presenter and a room.";

        if (error != null)
        {
            TempData["Error"] = error;
            return RedirectToAction(nameof(Create), new { investorId, sectorId, start = start.ToString("HH:mm") });
        }

        TempData["Message"] = "Reservation confirmed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();
        TempData["Message"] = "Reservation cancelled. The room, investor and presenter are free again at that time.";
        return RedirectToAction(nameof(Index));
    }
}
