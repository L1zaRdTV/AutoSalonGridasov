using System.Diagnostics;
using AutoSalonGrida.Data;
using AutoSalonGrida.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutoSalonGrida.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.PopularCars = await _context.Cars
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Images)
            .OrderByDescending(c => c.PopularityScore)
            .Take(6)
            .ToListAsync();

        ViewBag.Categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
