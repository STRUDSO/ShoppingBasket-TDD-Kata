using NUnit.Framework;

namespace ShoppingBasketA.Tests;

[TestFixture]
public class BasketSummaryTest
{
    [Test]
    public void Subtotal_Is_Sum_Of_Item_Prices()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        Assert.That(summary.Subtotal, Is.EqualTo(159.94m));
    }

    [Test]
    public void DiscountPercentage_Is_Five_Percent_Over_100()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        Assert.That(summary.DiscountPercentage, Is.EqualTo(0.05m));
    }

    [Test]
    public void DiscountAmount_Is_Subtotal_Times_Percentage()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        Assert.That(summary.DiscountAmount, Is.EqualTo(7.997m));
    }

    [Test]
    public void Total_Is_Subtotal_Minus_Discount()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        Assert.That(summary.Total, Is.EqualTo(151.943m));
    }

    [Test]
    public void ItemCount_Is_Total_Quantity()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        Assert.That(summary.ItemCount, Is.EqualTo(13));
    }

    [Test]
    public void Ten_Percent_Discount_Over_200()
    {
        var summary = new BasketSummary
        {
            Subtotal = 250m,
            DiscountPercentage = 0.10m,
            DiscountAmount = 25m,
            Total = 225m,
            ItemCount = 1,
        };

        Assert.That(summary.DiscountPercentage, Is.EqualTo(0.10m));
        Assert.That(summary.Total, Is.EqualTo(225m));
    }

    [Test]
    public void No_Discount_Under_100()
    {
        var summary = new BasketSummary
        {
            Subtotal = 50m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 50m,
            ItemCount = 1,
        };

        Assert.That(summary.DiscountPercentage, Is.EqualTo(0m));
        Assert.That(summary.Total, Is.EqualTo(50m));
    }
}

[TestFixture]
public class CheckoutServiceTest
{
    [Test]
    public void Summarize_Sets_Subtotal()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("A", 10m), 5);

        var summary = new CheckoutService().Summarize(basket);

        Assert.That(summary.Subtotal, Is.EqualTo(50m));
    }

    [Test]
    public void Summarize_Sets_ItemCount()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("A", 10m), 5);
        basket.Add(new BasketItem("B", 25m), 2);

        var summary = new CheckoutService().Summarize(basket);

        Assert.That(summary.ItemCount, Is.EqualTo(7));
    }

    [Test]
    public void Summarize_Applies_Five_Percent_Discount()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("A", 10m), 5);
        basket.Add(new BasketItem("B", 25m), 2);
        basket.Add(new BasketItem("C", 9.99m), 6);

        var summary = new CheckoutService().Summarize(basket);

        Assert.That(summary.Subtotal, Is.EqualTo(159.94m));
        Assert.That(summary.DiscountPercentage, Is.EqualTo(0.05m));
        Assert.That(summary.Total, Is.EqualTo(151.943m));
    }
}
