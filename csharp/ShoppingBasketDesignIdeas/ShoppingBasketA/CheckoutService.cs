namespace ShoppingBasketA;

public class CheckoutService
{
    public string GenerateReceipt(BasketSummary summary)
    {
        var lines = new List<string>
        {
            $"Items: {summary.ItemCount}",
            $"Subtotal: ${summary.Subtotal:F2}",
        };

        if (summary.DiscountPercentage > 0)
        {
            lines.Add($"Discount ({summary.DiscountPercentage:P0}): -${summary.DiscountAmount:F2}");
        }

        lines.Add($"Total: ${summary.Total:F2}");

        return string.Join(Environment.NewLine, lines);
    }

    public bool QualifiesForFreeShipping(BasketSummary summary)
    {
        return summary.Total >= 100m;
    }

    public string GetDiscountTier(BasketSummary summary)
    {
        return summary.DiscountPercentage switch
        {
            >= 0.10m => "Gold",
            >= 0.05m => "Silver",
            _ => "None",
        };
    }
}
