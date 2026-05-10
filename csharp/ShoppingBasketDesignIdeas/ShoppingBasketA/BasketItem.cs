namespace ShoppingBasketA;

public class BasketItem
{
    public string Name { get; }
    public decimal Price { get; }

    public BasketItem(string name, decimal price)
    {
        Name = name;
        Price = price;
    }
}
