namespace AutoSalonGrida.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = OrderStatuses.Pending;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
