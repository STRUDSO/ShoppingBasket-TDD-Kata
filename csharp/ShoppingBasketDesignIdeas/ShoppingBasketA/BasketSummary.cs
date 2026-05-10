namespace ShoppingBasketA;

public class BasketSummary
{
    public decimal Subtotal { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}
