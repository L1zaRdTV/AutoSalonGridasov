using AutoSalonGrida.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSalonGrida.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _context;

    public OrdersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> My()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Challenge();

        var orders = await _context.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId)
            .Include(o => o.User)
            .Include(o => o.Items)
            .ThenInclude(i => i.Car)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
}
