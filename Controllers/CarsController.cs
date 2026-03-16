using AutoSalonGrida.Data;
using AutoSalonGrida.Models.ViewModels;
using AutoSalonGrida.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace AutoSalonGrida.Controllers;

[AllowAnonymous]
public class CarsController : Controller
{
    private const int MinGalleryImagesCount = 3;
    private readonly ApplicationDbContext _context;
    private readonly ICarPhotoService _carPhotoService;

    public CarsController(ApplicationDbContext context, ICarPhotoService carPhotoService)
    {
        _context = context;
        _carPhotoService = carPhotoService;
    }

    public async Task<IActionResult> Index(
        string? search,
        string? brand,
        int? categoryId,
        decimal? minPrice,
        decimal? maxPrice,
        int? minYear,
        int? maxYear,
        string? minProductionDate,
        string? maxProductionDate,
        int? minMileage,
        int? maxMileage,
        string? bodyType,
        string? engineType,
        string? sort)
    {
        var query = _context.Cars
            .AsNoTracking()
            .Include(c => c.Category)
            .Include(c => c.Images)
            .AsQueryable();

        var filterErrors = new List<string>();

        ValidateNonNegativeFilter(ref minPrice, "Цена от", filterErrors);
        ValidateNonNegativeFilter(ref maxPrice, "Цена до", filterErrors);
        ValidateNonNegativeFilter(ref minMileage, "Пробег от", filterErrors);
        ValidateNonNegativeFilter(ref maxMileage, "Пробег до", filterErrors);

        if (!string.IsNullOrWhiteSpace(minProductionDate))
        {
            if (TryParseProductionDate(minProductionDate, out var parsedMinDate))
            {
                minYear = parsedMinDate.Year;
            }
            else
            {
                filterErrors.Add($"Поле 'Дата выпуска от' содержит некорректное значение '{minProductionDate}'. Выберите дату из календаря.");
            }
        }

        if (!string.IsNullOrWhiteSpace(maxProductionDate))
        {
            if (TryParseProductionDate(maxProductionDate, out var parsedMaxDate))
            {
                maxYear = parsedMaxDate.Year;
            }
            else
            {
                filterErrors.Add($"Поле 'Дата выпуска до' содержит некорректное значение '{maxProductionDate}'. Выберите дату из календаря.");
            }
        }

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c => c.Brand.Contains(search) || c.Model.Contains(search));

        if (!string.IsNullOrWhiteSpace(brand)) query = query.Where(c => c.Brand == brand);
        if (categoryId.HasValue) query = query.Where(c => c.CategoryId == categoryId.Value);
        if (minPrice.HasValue) query = query.Where(c => c.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(c => c.Price <= maxPrice.Value);
        if (minYear.HasValue) query = query.Where(c => c.Year >= minYear.Value);
        if (maxYear.HasValue) query = query.Where(c => c.Year <= maxYear.Value);
        if (minMileage.HasValue) query = query.Where(c => c.Mileage >= minMileage.Value);
        if (maxMileage.HasValue) query = query.Where(c => c.Mileage <= maxMileage.Value);
        if (!string.IsNullOrWhiteSpace(bodyType)) query = query.Where(c => c.BodyType == bodyType);
        if (!string.IsNullOrWhiteSpace(engineType)) query = query.Where(c => c.EngineType == engineType);

        query = sort switch
        {
            "price_desc" => query.OrderByDescending(c => c.Price),
            "year_asc" => query.OrderBy(c => c.Year),
            "year_desc" => query.OrderByDescending(c => c.Year),
            _ => query.OrderBy(c => c.Price)
        };

        var brands = await _context.Cars.AsNoTracking().Select(c => c.Brand).Distinct().OrderBy(b => b).ToListAsync();
        var bodyTypes = await _context.Cars.AsNoTracking().Select(c => c.BodyType).Distinct().OrderBy(t => t).ToListAsync();
        var engineTypes = await _context.Cars.AsNoTracking().Select(c => c.EngineType).Distinct().OrderBy(t => t).ToListAsync();

        return View(new CarCatalogViewModel
        {
            Cars = await query.ToListAsync(),
            Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString()) { Selected = categoryId.HasValue && c.Id == categoryId.Value }).ToListAsync(),
            Brands = brands.Select(b => new SelectListItem(b, b) { Selected = b == brand }).ToList(),
            Search = search,
            Brand = brand,
            CategoryId = categoryId,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            MinYear = minYear,
            MaxYear = maxYear,
            MinProductionDate = minProductionDate,
            MaxProductionDate = maxProductionDate,
            MinMileage = minMileage,
            MaxMileage = maxMileage,
            BodyType = bodyType,
            EngineType = engineType,
            BodyTypes = bodyTypes.Select(t => new SelectListItem(t, t) { Selected = t == bodyType }).ToList(),
            EngineTypes = engineTypes.Select(t => new SelectListItem(t, t) { Selected = t == engineType }).ToList(),
            Sort = sort,
            FilterErrors = filterErrors
        });

    }

    public async Task<IActionResult> Details(int id)
    {
        var car = await _context.Cars.Include(c => c.Category).FirstOrDefaultAsync(c => c.Id == id);
        if (car is null) return NotFound();

        var gallery = await _context.CarImages.Where(i => i.CarId == id).ToListAsync();
        if (gallery.Count < MinGalleryImagesCount)
        {
            var photos = await _carPhotoService.GetPhotosAsync(car.Brand, car.Model, 4);
            var shouldSaveChanges = false;

            if (string.IsNullOrWhiteSpace(car.ImageUrl))
            {
                car.ImageUrl = photos[0];
                shouldSaveChanges = true;
            }

            var existing = gallery.Select(i => i.ImagePath).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var photo in photos.Skip(1))
            {
                if (existing.Contains(photo))
                {
                    continue;
                }

                var image = new Models.CarImage { CarId = car.Id, ImagePath = photo };
                _context.CarImages.Add(image);
                gallery.Add(image);
                existing.Add(photo);
                shouldSaveChanges = true;

                if (gallery.Count >= MinGalleryImagesCount)
                {
                    break;
                }
            }

            if (shouldSaveChanges)
            {
                await _context.SaveChangesAsync();
            }
        }

        if (gallery.Count == 0)
        {
            gallery.Add(new Models.CarImage { ImagePath = car.ImageUrl });
        }

        return View(new CarDetailsViewModel { Car = car, Gallery = gallery });
    }

    public async Task<IActionResult> Categories() => View(await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync());

    public async Task<IActionResult> Brands() => View(await _context.Cars.AsNoTracking().Select(c => c.Brand).Distinct().OrderBy(b => b).ToListAsync());

    private static bool TryParseProductionDate(string value, out DateTime parsedDate)
    {
        return DateTime.TryParseExact(
            value,
            ["yyyy-MM-dd", "dd.MM.yyyy"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }

    private static void ValidateNonNegativeFilter(ref decimal? value, string fieldName, ICollection<string> errors)
    {
        if (value.HasValue && value.Value < 0)
        {
            errors.Add($"Поле '{fieldName}' не может быть отрицательным.");
            value = null;
        }
    }

    private static void ValidateNonNegativeFilter(ref int? value, string fieldName, ICollection<string> errors)
    {
        if (value.HasValue && value.Value < 0)
        {
            errors.Add($"Поле '{fieldName}' не может быть отрицательным.");
            value = null;
        }
    }
}
