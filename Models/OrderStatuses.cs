namespace AutoSalonGrida.Models;

public static class OrderStatuses
{
    public const string Pending = "На рассмотрении";
    public const string Approved = "Подтвержден";
    public const string Cancelled = "Отменен";

    public static readonly string[] All = [Pending, Approved, Cancelled];
}
