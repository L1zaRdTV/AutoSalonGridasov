using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models;

public class CarImage
{
    public int Id { get; set; }

    public int CarId { get; set; }
    public Car? Car { get; set; }

    [Required, StringLength(250)]
    public string ImagePath { get; set; } = string.Empty;
}
