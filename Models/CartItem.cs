namespace AutoSalonGrida.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public Cart? Cart { get; set; }
    public int CarId { get; set; }
    public Car? Car { get; set; }
    public int Quantity { get; set; } = 1;
}
