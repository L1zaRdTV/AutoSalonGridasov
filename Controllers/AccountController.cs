using AutoSalonGrida.Data;
using AutoSalonGrida.Models;
using AutoSalonGrida.Models.ViewModels;
using AutoSalonGrida.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AutoSalonGrida.Controllers;

public class AccountController : Controller
{
    private const string AdminRoleSecret = "5890";
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".webp"
    };

    private readonly ApplicationDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly IWebHostEnvironment _environment;

    public AccountController(ApplicationDbContext context, IPasswordService passwordService, IWebHostEnvironment environment)
    {
        _context = context;
        _passwordService = passwordService;
        _environment = environment;
    }

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        ValidateLoginModel(model);
        if (!ModelState.IsValid) return View(model);

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user is null || !_passwordService.VerifyPassword(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
            return View(model);
        }

        await SignInAsync(user, model.RememberMe);
        return RedirectToLocal(returnUrl);
    }

    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        ValidateRegisterModel(model);

        if (model.Role != "Администратор")
        {
            model.AdminPassword = null;
            ModelState.Remove(nameof(model.AdminPassword));
        }

        if (model.Role == "Администратор" && model.AdminPassword != AdminRoleSecret)
        {
            ModelState.AddModelError(nameof(model.AdminPassword), "Неверный специальный код администратора.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Этот email уже используется.");
        }

        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Role = model.Role == "Администратор" ? "Admin" : "User",
            PasswordHash = _passwordService.HashPassword(model.Password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInAsync(user, false);
        return RedirectToAction("Index", "Home");
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        return View(new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            About = user.About,
            AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? "/assets/images/default-car.svg" : user.AvatarUrl,
            Roles = new List<string> { user.Role }
        });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null) return Challenge();

        model.Roles = new List<string> { user.Role };
        model.FullName = model.FullName?.Trim() ?? string.Empty;
        model.Email = model.Email?.Trim() ?? string.Empty;
        model.PhoneNumber = model.PhoneNumber?.Trim();
        model.City = model.City?.Trim();
        model.About = model.About?.Trim();

        if (!string.IsNullOrWhiteSpace(model.City) && !ProfileCities.Russian.Contains(model.City))
        {
            ModelState.AddModelError(nameof(model.City), "Выберите город только из списка.");
        }

        if (await _context.Users.AnyAsync(u => u.Email == model.Email && u.Id != user.Id))
        {
            ModelState.AddModelError(nameof(model.Email), "Этот email уже используется.");
        }

        var passwordChangeRequested =
            !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);

        if (passwordChangeRequested)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Введите текущий пароль.");
            }
            else if (!_passwordService.VerifyPassword(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Текущий пароль введён неверно.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Введите новый пароль.");
            }
            else if (model.NewPassword.Length < 15 || model.NewPassword.Length > 20)
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Новый пароль должен содержать от 15 до 20 символов.");
            }
            else if (string.Equals(model.NewPassword, model.CurrentPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Новый пароль должен отличаться от текущего.");
            }

            if (!string.Equals(model.NewPassword, model.ConfirmNewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "Подтверждение пароля не совпадает.");
            }
        }

        if (!ModelState.IsValid) return View(model);

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.PhoneNumber = model.PhoneNumber;
        user.City = model.City;
        user.About = model.About;

        if (model.AvatarFile is not null && model.AvatarFile.Length > 0)
        {
            var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
            if (!AllowedAvatarExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.AvatarFile), "Для аватарки разрешены только PNG и WEBP.");
                return View(model);
            }

            var avatarsDirectory = Path.Combine(_environment.WebRootPath, "images", "avatars");
            Directory.CreateDirectory(avatarsDirectory);

            var fileName = $"{user.Id}-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(avatarsDirectory, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await model.AvatarFile.CopyToAsync(stream);

            user.AvatarUrl = $"/images/avatars/{fileName}";
        }

        if (passwordChangeRequested && !string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = _passwordService.HashPassword(model.NewPassword);
        }

        await _context.SaveChangesAsync();
        await SignInAsync(user, true);

        TempData["ProfileMessage"] = "Профиль успешно обновлён.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private async Task<User?> GetCurrentUserAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return null;
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    private async Task SignInAsync(User user, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties { IsPersistent = isPersistent });
    }

    private void ValidateLoginModel(LoginViewModel model)
    {
        ValidateEmail(model.Email, nameof(model.Email), trimValue => model.Email = trimValue);
        ValidatePassword(model.Password, nameof(model.Password));
    }

    private void ValidateEmail(string email, string fieldName, Action<string> setValue)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(fieldName, "Введите адрес электронной почты.");
            return;
        }

        var trimmedEmail = email.Trim();
        setValue(trimmedEmail);

        if (trimmedEmail.Length > 254)
        {
            ModelState.AddModelError(fieldName, "Email не должен превышать 254 символа.");
        }
        else if (!Regex.IsMatch(trimmedEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
        {
            ModelState.AddModelError(fieldName, "Введите корректный адрес электронной почты.");
        }
    }

    private void ValidatePassword(string password, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(fieldName, "Введите пароль.");
        }
        else if (password.Length < 15 || password.Length > 20)
        {
            ModelState.AddModelError(fieldName, "Пароль должен содержать от 15 до 20 символов.");
        }
    }

    private void ValidateRegisterModel(RegisterViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.FullName))
        {
            ModelState.AddModelError(nameof(model.FullName), "Введите ФИО.");
        }
        else
        {
            model.FullName = model.FullName.Trim();
            if (model.FullName.Length > 25)
            {
                ModelState.AddModelError(nameof(model.FullName), "ФИО не должно превышать 25 символов.");
            }
            else if (!Regex.IsMatch(model.FullName, @"^[\p{L}\s\-']+$"))
            {
                ModelState.AddModelError(nameof(model.FullName), "ФИО содержит недопустимые символы. Разрешены только буквы, пробел, дефис и апостроф.");
            }
        }

        ValidateEmail(model.Email, nameof(model.Email), trimValue => model.Email = trimValue);
        ValidatePassword(model.Password, nameof(model.Password));

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "Введите номер телефона.");
        }
        else
        {
            model.PhoneNumber = model.PhoneNumber.Trim();
            if (model.PhoneNumber.Length is < 11 or > 12)
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Телефон должен содержать от 11 до 12 символов.");
            }
            else if (!Regex.IsMatch(model.PhoneNumber, @"^(\+7|8)\d{10}$"))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), "Неправильный формат телефона. Используйте +79991234567 или 89991234567.");
            }
        }

        if (string.IsNullOrWhiteSpace(model.ConfirmPassword))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Подтвердите пароль.");
        }
        else if (!string.Equals(model.Password, model.ConfirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(model.ConfirmPassword), "Пароли не совпадают.");
        }

        if (model.Role != "Пользователь" && model.Role != "Администратор")
        {
            ModelState.AddModelError(nameof(model.Role), "Некорректное значение роли.");
        }

        if (!string.IsNullOrWhiteSpace(model.AdminPassword) && !Regex.IsMatch(model.AdminPassword, @"^\d{4}$"))
        {
            ModelState.AddModelError(nameof(model.AdminPassword), "Код администратора должен содержать только цифры.");
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
