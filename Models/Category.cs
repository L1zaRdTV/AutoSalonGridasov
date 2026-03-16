using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(40)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Car> Cars { get; set; } = new List<Car>();
}
