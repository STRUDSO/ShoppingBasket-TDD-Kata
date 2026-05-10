namespace ShoppingBasketA;

public class CheckoutService
{
    public BasketSummary Summarize(ShoppingBasket basket)
    {
        var items = basket.GetItems();

        var subtotal = items.Sum(x => x.Item.Price * x.Quantity);
        var itemCount = items.Sum(x => x.Quantity);

        decimal discountPercentage = 0;
        if (subtotal > 200)
            discountPercentage = 0.10m;
        else if (subtotal > 100)
            discountPercentage = 0.05m;

        var discountAmount = subtotal * discountPercentage;
        var total = subtotal - discountAmount;

        return new BasketSummary
        {
            Subtotal = subtotal,
            DiscountPercentage = discountPercentage,
            DiscountAmount = discountAmount,
            Total = total,
            ItemCount = itemCount,
        };
    }
}
