using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoSalonGrida.Models.ViewModels;

public class ProfileViewModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Телефон")]
    public string? PhoneNumber { get; set; }

    [StringLength(100)]
    [Display(Name = "Город")]
    public string? City { get; set; }

    [StringLength(1000)]
    [Display(Name = "О себе")]
    public string? About { get; set; }

    public string AvatarUrl { get; set; } = "/images/cars/default-car.svg";

    [Display(Name = "Новая аватарка")]
    public IFormFile? AvatarFile { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}
