using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [StringLength(254, ErrorMessage = "Email не должен превышать 254 символа.")]
    [EmailAddress(ErrorMessage = "Неправильный формат email. Пример: name@example.com.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(20, MinimumLength = 15, ErrorMessage = "Пароль должен содержать от 15 до 20 символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Запомнить меня")]
    public bool RememberMe { get; set; }
}
