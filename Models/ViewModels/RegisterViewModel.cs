using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите ФИО.")]
    [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = "Email должен быть в формате example@mail.ru.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать не менее 6 символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Введите номер телефона.")]
    [Phone(ErrorMessage = "Введите корректный номер телефона.")]
    [RegularExpression(@"^(\+7|8)\d{10}$", ErrorMessage = "Телефон должен быть в формате +79991234567 или 89991234567.")]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [RegularExpression("^(Пользователь|Администратор)$", ErrorMessage = "Некорректное значение роли.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "Пользователь";

    [DataType(DataType.Password)]
    [Display(Name = "Специальный код администратора")]
    public string? AdminPassword { get; set; }
}
