using AutoSalonGrida.Data;
using AutoSalonGrida.Models;
using AutoSalonGrida.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AutoSalonGrida.Controllers;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await GetOrCreateCartAsync();
        var fullCart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Car)
            .FirstAsync(c => c.Id == cart.Id);

        return View(new CartViewModel { CartId = fullCart.Id, Items = fullCart.Items.ToList() });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int carId)
    {
        var cart = await GetOrCreateCartAsync();
        var item = await _context.CartItems.FirstOrDefaultAsync(i => i.CartId == cart.Id && i.CarId == carId);

        if (item is null)
        {
            _context.CartItems.Add(new CartItem { CartId = cart.Id, CarId = carId, Quantity = 1 });
        }
        else
        {
            item.Quantity++;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Автомобиль добавлен в корзину.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(int itemId, int quantity)
    {
        var item = await _context.CartItems.Include(i => i.Cart).FirstOrDefaultAsync(i => i.Id == itemId);
        var userId = GetCurrentUserId();
        if (item is null || item.Cart?.UserId != userId) return NotFound();

        item.Quantity = Math.Max(1, quantity);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int itemId)
    {
        var item = await _context.CartItems.Include(i => i.Cart).FirstOrDefaultAsync(i => i.Id == itemId);
        var userId = GetCurrentUserId();
        if (item is null || item.Cart?.UserId != userId) return NotFound();

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Позиция удалена из корзины.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout()
    {
        var cart = await GetOrCreateCartAsync();
        var fullCart = await _context.Carts.Include(c => c.Items).ThenInclude(i => i.Car).FirstAsync(c => c.Id == cart.Id);
        if (!fullCart.Items.Any())
        {
            TempData["Success"] = "Корзина пуста.";
            return RedirectToAction(nameof(Index));
        }

        var order = new Order
        {
            UserId = fullCart.UserId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatuses.Pending,
            TotalPrice = fullCart.Items.Sum(i => (i.Car?.Price ?? 0) * i.Quantity)
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var orderItems = fullCart.Items.Select(item => new OrderItem
        {
            OrderId = order.Id,
            CarId = item.CarId,
            Quantity = item.Quantity,
            UnitPrice = item.Car?.Price ?? 0
        });

        _context.OrderItems.AddRange(orderItems);
        _context.CartItems.RemoveRange(fullCart.Items);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Заказ отправлен на рассмотрение администратору. Статус можно отслеживать в разделе заказов.";
        return RedirectToAction("My", "Orders");
    }

    private async Task<Cart> GetOrCreateCartAsync()
    {
        var userId = GetCurrentUserId();
        var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart is not null) return cart;

        cart = new Cart { UserId = userId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return cart;
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new InvalidOperationException("Authenticated user id is missing.");
        }

        return userId;
    }
}
