namespace AutoSalonGrida.Models.ViewModels;

public class CarDetailsViewModel
{
    public Car Car { get; set; } = default!;
    public List<CarImage> Gallery { get; set; } = [];
}
