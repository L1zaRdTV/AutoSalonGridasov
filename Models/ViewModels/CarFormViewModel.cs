using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSalonGrida.Models.ViewModels;

public class CarFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(60)]
    public string Brand { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [Range(0, 100000000)]
    public decimal Price { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Required, StringLength(25)]
    public string BodyType { get; set; } = string.Empty;

    [StringLength(2500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 2000000)]
    public int Mileage { get; set; }

    [Required, StringLength(80)]
    public string EngineType { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ImageUrl { get; set; }

    public IFormFile? MainImage { get; set; }
    public List<IFormFile>? GalleryImages { get; set; }

    public List<SelectListItem> CategoryOptions { get; set; } = [];
}
