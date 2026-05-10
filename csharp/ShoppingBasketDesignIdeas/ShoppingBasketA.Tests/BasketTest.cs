using NUnit.Framework;

namespace ShoppingBasketA.Tests;

[TestFixture]
public class CheckoutServiceTest
{
    private readonly CheckoutService _checkout = new();

    [Test]
    public void Receipt_Shows_Subtotal()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        var receipt = _checkout.GenerateReceipt(summary);

        Assert.That(receipt, Does.Contain("Subtotal: $159.94"));
    }

    [Test]
    public void Receipt_Shows_Discount_Line_When_Discounted()
    {
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 7.997m,
            Total = 151.943m,
            ItemCount = 13,
        };

        var receipt = _checkout.GenerateReceipt(summary);

        Assert.That(receipt, Does.Contain("Discount (5%)"));
    }

    [Test]
    public void Receipt_Hides_Discount_Line_When_No_Discount()
    {
        var summary = new BasketSummary
        {
            Subtotal = 50m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 50m,
            ItemCount = 1,
        };

        var receipt = _checkout.GenerateReceipt(summary);

        Assert.That(receipt, Does.Not.Contain("Discount"));
    }

    [Test]
    public void Free_Shipping_When_Total_Over_100()
    {
        // NOTE: Subtotal is 80 but Total is 120 — impossible in production
        // (discount can only reduce total below subtotal, never increase it)
        // but the test is green because CheckoutService never validates consistency
        var summary = new BasketSummary
        {
            Subtotal = 80m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 120m,
            ItemCount = 2,
        };

        Assert.That(_checkout.QualifiesForFreeShipping(summary), Is.True);
    }

    [Test]
    public void No_Free_Shipping_When_Total_Under_100()
    {
        var summary = new BasketSummary
        {
            Subtotal = 90m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 90m,
            ItemCount = 3,
        };

        Assert.That(_checkout.QualifiesForFreeShipping(summary), Is.False);
    }

    [Test]
    public void Gold_Tier_When_Ten_Percent_Discount()
    {
        var summary = new BasketSummary
        {
            Subtotal = 250m,
            DiscountPercentage = 0.10m,
            DiscountAmount = 25m,
            Total = 225m,
            ItemCount = 1,
        };

        Assert.That(_checkout.GetDiscountTier(summary), Is.EqualTo("Gold"));
    }

    [Test]
    public void Silver_Tier_When_Five_Percent_Discount()
    {
        // NOTE: DiscountAmount doesn't match Subtotal * DiscountPercentage
        // (159.94 * 0.05 = 7.997, not 10.00) — but the test passes
        var summary = new BasketSummary
        {
            Subtotal = 159.94m,
            DiscountPercentage = 0.05m,
            DiscountAmount = 10.00m,
            Total = 149.94m,
            ItemCount = 5,
        };

        Assert.That(_checkout.GetDiscountTier(summary), Is.EqualTo("Silver"));
    }

    [Test]
    public void No_Tier_When_No_Discount()
    {
        var summary = new BasketSummary
        {
            Subtotal = 50m,
            DiscountPercentage = 0m,
            DiscountAmount = 0m,
            Total = 50m,
            ItemCount = 1,
        };

        Assert.That(_checkout.GetDiscountTier(summary), Is.EqualTo("None"));
    }
}

[TestFixture]
public class PricingServiceTest
{
    [Test]
    public void Five_Percent_Discount_Over_100()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("A", 10m), 5);
        basket.Add(new BasketItem("B", 25m), 2);
        basket.Add(new BasketItem("C", 9.99m), 6);

        var summary = new PricingService().Calculate(basket);

        Assert.That(summary.Subtotal, Is.EqualTo(159.94m));
        Assert.That(summary.DiscountPercentage, Is.EqualTo(0.05m));
        Assert.That(summary.Total, Is.EqualTo(151.943m));
    }

    [Test]
    public void Ten_Percent_Discount_Over_200()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("E", 250m), 1);

        var summary = new PricingService().Calculate(basket);

        Assert.That(summary.DiscountPercentage, Is.EqualTo(0.10m));
        Assert.That(summary.Total, Is.EqualTo(225m));
    }

    [Test]
    public void No_Discount_Under_100()
    {
        var basket = new ShoppingBasket();
        basket.Add(new BasketItem("A", 10m), 5);

        var summary = new PricingService().Calculate(basket);

        Assert.That(summary.DiscountPercentage, Is.EqualTo(0m));
        Assert.That(summary.Total, Is.EqualTo(50m));
    }
}
