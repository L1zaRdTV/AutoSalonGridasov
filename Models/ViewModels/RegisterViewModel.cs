using System.ComponentModel.DataAnnotations;

namespace AutoSalonGrida.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введите ФИО.")]
    [StringLength(100, ErrorMessage = "ФИО не должно превышать 100 символов.")]
    [RegularExpression(@"^[\p{L}\s\-']+$", ErrorMessage = "ФИО содержит недопустимый символ. Разрешены только буквы, пробел, дефис и апостроф.")]
    [Display(Name = "ФИО")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите адрес электронной почты.")]
    [StringLength(254, ErrorMessage = "Email не должен превышать 254 символа.")]
    [EmailAddress(ErrorMessage = "Введите корректный адрес электронной почты.")]
    [RegularExpression(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", ErrorMessage = "Email должен быть в формате example@mail.ru.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен содержать от 6 до 100 символов.")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;


    [Required(ErrorMessage = "Введите номер телефона.")]
    [StringLength(12, MinimumLength = 11, ErrorMessage = "Телефон должен содержать от 11 до 12 символов.")]
    [Phone(ErrorMessage = "Введите корректный номер телефона.")]
    [RegularExpression(@"^(\+7|8)\d{10}$", ErrorMessage = "Телефон должен быть в формате +79991234567 или 89991234567.")]
    [Display(Name = "Номер телефона")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Подтверждение пароля должно содержать от 6 до 100 символов.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
    [Display(Name = "Подтверждение пароля")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Выберите роль.")]
    [RegularExpression("^(Пользователь|Администратор)$", ErrorMessage = "Некорректное значение роли.")]
    [Display(Name = "Роль")]
    public string Role { get; set; } = "Пользователь";

    [DataType(DataType.Password)]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Код администратора должен состоять из 4 символов.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Код администратора должен содержать только цифры.")]
    [Display(Name = "Специальный код администратора")]
    public string? AdminPassword { get; set; }
}
