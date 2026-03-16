using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите ФИО.")]
    [StringLength(25, ErrorMessage = "ФИО не должно превышать 25 символов.")]
    [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "ФИО содержит недопустимые символы. Разрешены только буквы, пробел, дефис и апостроф.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

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

    [Required(ErrorMessage = "Введите номер телефона.")]
    [StringLength(12, MinimumLength = 11, ErrorMessage = "Телефон должен содержать от 11 до 12 символов.")]
    [RegularExpression(@"^(\+7|8)\d{10}$", ErrorMessage = "Неправильный формат телефона. Используйте +79991234567 или 89991234567.")]
    [Display(Name = "Телефон")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль.")]
    [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают.")]
    [DataType(DataType.Password)]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "Пользователь";

    [StringLength(4, MinimumLength = 4, ErrorMessage = "Код администратора должен содержать 4 цифры.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Код администратора должен содержать только цифры.")]
    [Display(Name = "Код администратора")]
    public string? AdminPassword { get; set; }
}
