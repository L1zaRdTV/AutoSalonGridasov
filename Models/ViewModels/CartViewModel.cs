namespace AutoSalonGrida.Models.ViewModels;

public class CartViewModel
{
    public int CartId { get; set; }
    public List<CartItem> Items { get; set; } = [];
    public decimal TotalPrice => Items.Sum(i => (i.Car?.Price ?? 0) * i.Quantity);
}
