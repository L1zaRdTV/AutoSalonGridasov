using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSalonGrida.Models.ViewModels;

public class CarFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Укажите марку автомобиля.")]
    [StringLength(60, ErrorMessage = "Марка не должна превышать 60 символов.")]
    [RegularExpression(@"^[\p{L}\d\s\-]+$", ErrorMessage = "Марка может содержать только буквы, цифры, пробел и дефис.")]
    public string Brand { get; set; } = string.Empty;

    [Required(ErrorMessage = "Укажите модель автомобиля.")]
    [StringLength(80, ErrorMessage = "Модель не должна превышать 80 символов.")]
    [RegularExpression(@"^[\p{L}\d\s\-]+$", ErrorMessage = "Модель может содержать только буквы, цифры, пробел и дефис.")]
    public string Model { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите дату выпуска.")]
    [RegularExpression(@"^(\d{4}-\d{2}-\d{2}|\d{2}\.\d{2}\.\d{4})$", ErrorMessage = "Дата выпуска указана неверно. Используйте формат ГГГГ-ММ-ДД или ДД.ММ.ГГГГ.")]
    public string ProductionDate { get; set; } = $"{DateTime.UtcNow:yyyy}-01-01";

    [Range(1990, 2100, ErrorMessage = "Год выпуска должен быть в диапазоне от 1990 до 2100.")]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Range(0, 100000000, ErrorMessage = "Цена должна быть в диапазоне от 0 до 100000000.")]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Укажите тип кузова.")]
    [StringLength(25, ErrorMessage = "Тип кузова не должен превышать 25 символов.")]
    [RegularExpression(@"^[\p{L}\s\-]+$", ErrorMessage = "Тип кузова может содержать только буквы, пробел и дефис.")]
    public string BodyType { get; set; } = string.Empty;

    [StringLength(2500, ErrorMessage = "Описание не должно превышать 2500 символов.")]
    public string Description { get; set; } = string.Empty;

    [Range(0, 2000000, ErrorMessage = "Пробег должен быть в диапазоне от 0 до 2000000.")]
    public int Mileage { get; set; }

    [Required(ErrorMessage = "Укажите тип двигателя.")]
    [StringLength(80, ErrorMessage = "Тип двигателя не должен превышать 80 символов.")]
    [RegularExpression(@"^[\p{L}\d\s\-\+]+$", ErrorMessage = "Тип двигателя может содержать только буквы, цифры, пробел, дефис и знак +.")]
    public string EngineType { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Ссылка на изображение не должна превышать 250 символов.")]
    public string? ImageUrl { get; set; }

    public IFormFile? MainImage { get; set; }
    public List<IFormFile>? GalleryImages { get; set; }
    public List<int> RemoveImageIds { get; set; } = [];
    public List<CarImageViewModel> ExistingGallery { get; set; } = [];

    public List<SelectListItem> CategoryOptions { get; set; } = [];
}

public class CarImageViewModel
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
}
