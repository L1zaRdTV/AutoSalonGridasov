using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(254)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    [StringLength(12)]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(1000)]
    public string? About { get; set; }

    [StringLength(500)]
    public string AvatarUrl { get; set; } = "/images/cars/default-car.svg";

    public ICollection<Cart> Carts { get; set; } = new List<Cart>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
