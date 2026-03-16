using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [Display(Name = "Email")]
    [StringLength(254, ErrorMessage = "Email не должен превышать 254 символа.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [Display(Name = "Пароль")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов.")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}
