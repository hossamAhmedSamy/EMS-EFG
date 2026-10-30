using EMS.Data;
using EMS.Models;
using EMS.Services;
using EMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers;

public class PresenterController : Controller
{
    private readonly AppDbContext _context;

    public PresenterController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var presenters = await _context.Presenters
            .Include(p => p.Interests)
                .ThenInclude(x => x.Sector)
            .OrderBy(p => p.Name)
            .ToListAsync();
        return View(presenters);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Presenter presenter)
    {
        if (!ModelState.IsValid)
        {
            return View(presenter);
        }

        _context.Presenters.Add(presenter);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Manage), new { id = presenter.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var presenter = await _context.Presenters.FindAsync(id);
        if (presenter == null)
        {
            return NotFound();
        }
        return View(presenter);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Presenter input)
    {
        var presenter = await _context.Presenters.FindAsync(id);
        if (presenter == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            input.Id = id;
            return View(input);
        }

        presenter.Name = input.Name;
        presenter.Mobile = input.Mobile;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Presenter updated.";
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var presenter = await _context.Presenters.FindAsync(id);
        if (presenter == null)
        {
            return NotFound();
        }

        if (await _context.Reservations.AnyAsync(r => r.PresenterId == id))
        {
            TempData["Error"] = "This presenter has reservations. Cancel them before deleting the presenter.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        _context.Presenters.Remove(presenter);
        await _context.SaveChangesAsync();
        TempData["Message"] = $"Presenter \"{presenter.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Manage(int id)
    {
        var presenter = await _context.Presenters
            .Include(p => p.Interests)
                .ThenInclude(x => x.Sector)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (presenter == null)
        {
            return NotFound();
        }

        return View(new PresenterManageViewModel
        {
            Presenter = presenter,
            Sectors = await _context.Sectors.OrderBy(s => s.Name).ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInterest(int presenterId, int sectorId, TimeOnly from, TimeOnly to)
    {
        if (!await _context.Presenters.AnyAsync(p => p.Id == presenterId))
        {
            return NotFound();
        }

        var error = ModelState.IsValid
            ? await ValidateWindowAsync(presenterId, sectorId, from, to)
            : "Please choose a sector and enter both times.";

        if (error != null)
        {
            TempData["Error"] = error;
        }
        else
        {
            _context.PresenterInterests.Add(new PresenterInterest
            {
                PresenterId = presenterId,
                SectorId = sectorId,
                StartTime = from,
                EndTime = to
            });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Time window added.";
        }

        return RedirectToAction(nameof(Manage), new { id = presenterId });
    }

    [HttpGet]
    public async Task<IActionResult> EditInterest(int id)
    {
        var interest = await LoadInterestAsync(id);
        if (interest == null)
        {
            return NotFound();
        }
        return View(interest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInterest(int id, TimeOnly from, TimeOnly to)
    {
        var interest = await LoadInterestAsync(id);
        if (interest == null)
        {
            return NotFound();
        }

        var error = ModelState.IsValid
            ? await ValidateWindowAsync(interest.PresenterId, interest.SectorId, from, to, interest.Id)
            : "Please enter both times.";

        if (error != null)
        {
            ViewData["Error"] = error;
            return View(interest);
        }

        interest.StartTime = from;
        interest.EndTime = to;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Time window updated.";
        return RedirectToAction(nameof(Manage), new { id = interest.PresenterId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInterest(int interestId)
    {
        var interest = await _context.PresenterInterests.FindAsync(interestId);
        if (interest == null)
        {
            return NotFound();
        }

        var remaining = await OtherWindowsAsync(interest.PresenterId, interest.SectorId, interest.Id);
        var booked = await BookedInSectorAsync(interest.PresenterId, interest.SectorId);

        if (!MatchingService.AllCovered(booked, remaining))
        {
            TempData["Error"] = "A reservation depends on this time window. Cancel it before removing the window.";
        }
        else
        {
            _context.PresenterInterests.Remove(interest);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Time window removed.";
        }

        return RedirectToAction(nameof(Manage), new { id = interest.PresenterId });
    }

    private Task<PresenterInterest?> LoadInterestAsync(int id) =>
        _context.PresenterInterests
            .Include(x => x.Sector)
            .Include(x => x.Presenter)
            .FirstOrDefaultAsync(x => x.Id == id);

    // Rules for adding or editing a window: valid length, no overlap with another window
    // in the same sector, and every existing reservation in that sector stays inside a window.
    private async Task<string?> ValidateWindowAsync(int presenterId, int sectorId, TimeOnly from, TimeOnly to, int excludeInterestId = 0)
    {
        var error = TimeRange.Validate(from, to);
        if (error != null)
            return error;

        if (!await _context.Sectors.AnyAsync(s => s.Id == sectorId))
            return "Please choose a sector.";

        var window = new TimeRange(from, to);
        var others = await OtherWindowsAsync(presenterId, sectorId, excludeInterestId);

        foreach (var other in others)
        {
            if (other.Overlaps(window))
                return $"This overlaps another window in the same sector ({other}). Edit that window instead.";
        }

        var booked = await BookedInSectorAsync(presenterId, sectorId);
        if (!MatchingService.AllCovered(booked, others.Append(window)))
            return "An existing reservation would fall outside this window. Cancel it first.";

        return null;
    }

    private Task<List<TimeRange>> OtherWindowsAsync(int presenterId, int sectorId, int excludeInterestId) =>
        _context.PresenterInterests
            .Where(x => x.PresenterId == presenterId && x.SectorId == sectorId && x.Id != excludeInterestId)
            .Select(x => new TimeRange(x.StartTime, x.EndTime))
            .ToListAsync();

    private Task<List<TimeRange>> BookedInSectorAsync(int presenterId, int sectorId) =>
        _context.Reservations
            .Where(r => r.PresenterId == presenterId && r.SectorId == sectorId)
            .Select(r => new TimeRange(r.StartTime, r.EndTime))
            .ToListAsync();
}
