using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AutoSalonGrida.Models.ViewModels;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Введите ФИО.")]
    [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов.")]
    [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "ФИО содержит недопустимый символ. Разрешены только буквы, пробел, дефис и апостроф.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [StringLength(254, ErrorMessage = "Email не должен превышать 254 символа.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(12, MinimumLength = 11, ErrorMessage = "Телефон должен содержать от 11 до 12 символов.")]
    [Phone(ErrorMessage = "Введите корректный номер телефона.")]
    [RegularExpression(@"^(\+7|8)\d{10}$", ErrorMessage = "Телефон должен быть в формате +79991234567 или 89991234567.")]
    [Display(Name = "Телефон")]
    public string? PhoneNumber { get; set; }

    [StringLength(100, ErrorMessage = "Название города не должно превышать 100 символов.")]
    [Display(Name = "Город")]
    public string? City { get; set; }

    [StringLength(1000, ErrorMessage = "Раздел 'О себе' не должен превышать 1000 символов.")]
    [Display(Name = "О себе")]
    public string? About { get; set; }

    public string AvatarUrl { get; set; } = "/images/cars/default-car.svg";

    [Display(Name = "Новая аватарка")]
    public IFormFile? AvatarFile { get; set; }

    public IList<string> Roles { get; set; } = new List<string>();
}
