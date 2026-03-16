using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models;

public class Car
{
    public int Id { get; set; }

    [Required, StringLength(60)]
    public string Brand { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Model { get; set; } = string.Empty;

    [Range(1990, 2100)]
    public int Year { get; set; }

    [Range(0, 100000000)]
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required, StringLength(25)]
    public string BodyType { get; set; } = string.Empty;

    [StringLength(2500)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 2000000)]
    public int Mileage { get; set; }

    [Required, StringLength(80)]
    public string EngineType { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string ImageUrl { get; set; } = string.Empty;

    public int PopularityScore { get; set; } = 1;

    public ICollection<CarImage> Images { get; set; } = new List<CarImage>();
}
