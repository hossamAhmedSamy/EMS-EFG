using EMS.Data;
using EMS.Models;
using EMS.Services;
using EMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EMS.Controllers;

public class InvestorController : Controller
{
    private readonly AppDbContext _context;

    public InvestorController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var investors = await _context.Investors
            .Include(i => i.Interests)
                .ThenInclude(x => x.Sector)
            .OrderBy(i => i.Name)
            .ToListAsync();
        return View(investors);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Investor investor)
    {
        if (!ModelState.IsValid)
        {
            return View(investor);
        }

        _context.Investors.Add(investor);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Manage), new { id = investor.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var investor = await _context.Investors.FindAsync(id);
        if (investor == null)
        {
            return NotFound();
        }
        return View(investor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Investor input)
    {
        var investor = await _context.Investors.FindAsync(id);
        if (investor == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            input.Id = id;
            return View(input);
        }

        investor.Name = input.Name;
        investor.Mobile = input.Mobile;
        await _context.SaveChangesAsync();
        TempData["Message"] = "Investor updated.";
        return RedirectToAction(nameof(Manage), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var investor = await _context.Investors.FindAsync(id);
        if (investor == null)
        {
            return NotFound();
        }

        if (await _context.Reservations.AnyAsync(r => r.InvestorId == id))
        {
            TempData["Error"] = "This investor has reservations. Cancel them before deleting the investor.";
            return RedirectToAction(nameof(Manage), new { id });
        }

        _context.Investors.Remove(investor);
        await _context.SaveChangesAsync();
        TempData["Message"] = $"Investor \"{investor.Name}\" deleted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Manage(int id)
    {
        var investor = await _context.Investors
            .Include(i => i.Interests)
                .ThenInclude(x => x.Sector)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (investor == null)
        {
            return NotFound();
        }

        return View(new InvestorManageViewModel
        {
            Investor = investor,
            Sectors = await _context.Sectors.OrderBy(s => s.Name).ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInterest(int investorId, int sectorId, TimeOnly from, TimeOnly to)
    {
        if (!await _context.Investors.AnyAsync(i => i.Id == investorId))
        {
            return NotFound();
        }

        var error = ModelState.IsValid
            ? await ValidateWindowAsync(investorId, sectorId, from, to)
            : "Please choose a sector and enter both times.";

        if (error != null)
        {
            TempData["Error"] = error;
        }
        else
        {
            _context.InvestorInterests.Add(new InvestorInterest
            {
                InvestorId = investorId,
                SectorId = sectorId,
                StartTime = from,
                EndTime = to
            });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Time window added.";
        }

        return RedirectToAction(nameof(Manage), new { id = investorId });
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
            ? await ValidateWindowAsync(interest.InvestorId, interest.SectorId, from, to, interest.Id)
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
        return RedirectToAction(nameof(Manage), new { id = interest.InvestorId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveInterest(int interestId)
    {
        var interest = await _context.InvestorInterests.FindAsync(interestId);
        if (interest == null)
        {
            return NotFound();
        }

        var remaining = await OtherWindowsAsync(interest.InvestorId, interest.SectorId, interest.Id);
        var booked = await BookedInSectorAsync(interest.InvestorId, interest.SectorId);

        if (!MatchingService.AllCovered(booked, remaining))
        {
            TempData["Error"] = "A reservation depends on this time window. Cancel it before removing the window.";
        }
        else
        {
            _context.InvestorInterests.Remove(interest);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Time window removed.";
        }

        return RedirectToAction(nameof(Manage), new { id = interest.InvestorId });
    }

    private Task<InvestorInterest?> LoadInterestAsync(int id) =>
        _context.InvestorInterests
            .Include(x => x.Sector)
            .Include(x => x.Investor)
            .FirstOrDefaultAsync(x => x.Id == id);

    // Rules for adding or editing a window: valid length, no overlap with another window
    // in the same sector, and every existing reservation in that sector stays inside a window.
    private async Task<string?> ValidateWindowAsync(int investorId, int sectorId, TimeOnly from, TimeOnly to, int excludeInterestId = 0)
    {
        var error = TimeRange.Validate(from, to);
        if (error != null)
            return error;

        if (!await _context.Sectors.AnyAsync(s => s.Id == sectorId))
            return "Please choose a sector.";

        var window = new TimeRange(from, to);
        var others = await OtherWindowsAsync(investorId, sectorId, excludeInterestId);

        foreach (var other in others)
        {
            if (other.Overlaps(window))
                return $"This overlaps another window in the same sector ({other}). Edit that window instead.";
        }

        var booked = await BookedInSectorAsync(investorId, sectorId);
        if (!MatchingService.AllCovered(booked, others.Append(window)))
            return "An existing reservation would fall outside this window. Cancel it first.";

        return null;
    }

    private Task<List<TimeRange>> OtherWindowsAsync(int investorId, int sectorId, int excludeInterestId) =>
        _context.InvestorInterests
            .Where(x => x.InvestorId == investorId && x.SectorId == sectorId && x.Id != excludeInterestId)
            .Select(x => new TimeRange(x.StartTime, x.EndTime))
            .ToListAsync();

    private Task<List<TimeRange>> BookedInSectorAsync(int investorId, int sectorId) =>
        _context.Reservations
            .Where(r => r.InvestorId == investorId && r.SectorId == sectorId)
            .Select(r => new TimeRange(r.StartTime, r.EndTime))
            .ToListAsync();
}
