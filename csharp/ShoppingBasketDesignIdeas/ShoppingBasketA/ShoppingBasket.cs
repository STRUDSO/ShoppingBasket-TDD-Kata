namespace ShoppingBasketA;

public class ShoppingBasket
{
    private readonly List<(BasketItem Item, int Quantity)> _items = new();

    public void Add(BasketItem item, int quantity)
    {
        _items.Add((item, quantity));
    }

    public int GetQuantity(string itemName)
    {
        return _items
            .Where(x => x.Item.Name == itemName)
            .Sum(x => x.Quantity);
    }

    public List<(BasketItem Item, int Quantity)> GetItems() => _items;
}
