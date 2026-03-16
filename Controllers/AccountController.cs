using AutoSalonGrida.Models;
using AutoSalonGrida.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace AutoSalonGrida.Controllers;

public class AccountController : Controller
{
    private const string AdminRoleSecret = "5890";
    private const string CityClaimType = "profile:city";
    private const string AboutClaimType = "profile:about";
    private const string AvatarClaimType = "profile:avatar";
    private static readonly HashSet<string> AllowedAvatarExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _environment;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Неверный email или пароль.");
        return View(model);
    }

    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role != "Администратор")
        {
            model.AdminPassword = null;
            ModelState.Remove(nameof(model.AdminPassword));
        }

        if (model.Role == "Администратор" && model.AdminPassword != AdminRoleSecret)
        {
            ModelState.AddModelError(nameof(model.AdminPassword), "Неверный специальный код администратора.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            var role = model.Role == "Администратор" ? "Admin" : "User";
            await _userManager.AddToRoleAsync(user, role);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
        }

        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        var model = new ProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            City = claims.FirstOrDefault(c => c.Type == CityClaimType)?.Value,
            About = claims.FirstOrDefault(c => c.Type == AboutClaimType)?.Value,
            AvatarUrl = claims.FirstOrDefault(c => c.Type == AvatarClaimType)?.Value ?? "/images/cars/default-car.svg",
            Roles = roles
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        var roles = await _userManager.GetRolesAsync(user);
        model.Roles = roles;

        if (!string.IsNullOrWhiteSpace(model.City) && !ProfileCities.Russian.Contains(model.City))
        {
            ModelState.AddModelError(nameof(model.City), "Выберите город только из списка.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.UserName = model.Email;
        user.PhoneNumber = model.PhoneNumber;

        if (model.AvatarFile is not null && model.AvatarFile.Length > 0)
        {
            var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();
            if (!AllowedAvatarExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.AvatarFile), "Разрешены только JPG, PNG и WEBP.");
                return View(model);
            }

            var avatarsDirectory = Path.Combine(_environment.WebRootPath, "images", "avatars");
            Directory.CreateDirectory(avatarsDirectory);

            var fileName = $"{user.Id}-{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(avatarsDirectory, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await model.AvatarFile.CopyToAsync(stream);

            model.AvatarUrl = $"/images/avatars/{fileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error));
            }

            return View(model);
        }

        await SetUserClaimAsync(user, CityClaimType, model.City);
        await SetUserClaimAsync(user, AboutClaimType, model.About);
        await SetUserClaimAsync(user, AvatarClaimType, model.AvatarUrl);

        TempData["ProfileMessage"] = "Профиль успешно обновлён.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied() => View();

    private static string TranslateIdentityError(IdentityError error)
    {
        return error.Code switch
        {
            "DuplicateUserName" => "Пользователь с таким email уже существует.",
            "DuplicateEmail" => "Этот email уже используется.",
            "InvalidEmail" => "Некорректный формат email.",
            "PasswordTooShort" => "Пароль слишком короткий.",
            "PasswordRequiresNonAlphanumeric" => "Пароль должен содержать хотя бы один спецсимвол.",
            "PasswordRequiresDigit" => "Пароль должен содержать хотя бы одну цифру.",
            "PasswordRequiresLower" => "Пароль должен содержать хотя бы одну строчную букву.",
            "PasswordRequiresUpper" => "Пароль должен содержать хотя бы одну заглавную букву.",
            _ => NormalizeIdentityDescription(error.Description)
        };
    }

    private static string NormalizeIdentityDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return "Произошла ошибка при обработке запроса.";
        }

        if (Regex.IsMatch(description, @"^[\u0400-\u04FF\s\p{P}\d]+$"))
        {
            return description;
        }

        return "Произошла ошибка при обработке запроса. Проверьте введённые данные.";
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    private async Task SetUserClaimAsync(ApplicationUser user, string claimType, string? value)
    {
        var claims = await _userManager.GetClaimsAsync(user);
        var existingClaim = claims.FirstOrDefault(c => c.Type == claimType);

        if (string.IsNullOrWhiteSpace(value))
        {
            if (existingClaim is not null)
            {
                await _userManager.RemoveClaimAsync(user, existingClaim);
            }

            return;
        }

        var newClaim = new Claim(claimType, value);
        if (existingClaim is null)
        {
            await _userManager.AddClaimAsync(user, newClaim);
        }
        else
        {
            await _userManager.ReplaceClaimAsync(user, existingClaim, newClaim);
        }
    }
}
