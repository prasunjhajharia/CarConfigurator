using CarConfigurator.Domain.Entities;
using CarConfigurator.Domain.Pricing;
using Xunit;

namespace CarConfigurator.Tests;

public class PriceCalculatorTests
{
    private static Trim CreateTrim()
    {
        var trim = new Trim { Id = 1, Name = "Style", BasePrice = 300000m };

        var red = new Option { Id = 1, Name = "Red", Price = 7500m };
        var wheels = new Option { Id = 2, Name = "18-inch alloy", Price = 9000m };
        var leather = new Option { Id = 3, Name = "Leather seats", Price = 14000m };

        trim.TrimOptions.Add(new TrimOption { TrimId = 1, Trim = trim, OptionId = 1, Option = red });
        trim.TrimOptions.Add(new TrimOption { TrimId = 1, Trim = trim, OptionId = 2, Option = wheels });
        trim.TrimOptions.Add(new TrimOption { TrimId = 1, Trim = trim, OptionId = 3, Option = leather, PriceOverride = 0m });

        return trim;
    }

    [Fact]
    public void No_options_returns_base_price()
    {
        var trim = CreateTrim();

        var result = PriceCalculator.Calculate(trim, new List<int>());

        Assert.Equal(300000m, result.Total);
    }

    [Fact]
    public void Options_are_added_to_base_price()
    {
        var trim = CreateTrim();

        var result = PriceCalculator.Calculate(trim, new[] { 1, 2 });

        Assert.Equal(316500m, result.Total);
        Assert.Equal(2, result.Lines.Count);
    }

    [Fact]
    public void Price_override_replaces_normal_option_price()
    {
        var trim = CreateTrim();

        var result = PriceCalculator.Calculate(trim, new[] { 3 });

        Assert.Equal(300000m, result.Total);
        Assert.Equal(0m, result.Lines[0].Price);
    }

    [Fact]
    public void Same_option_selected_twice_is_counted_once()
    {
        var trim = CreateTrim();

        var result = PriceCalculator.Calculate(trim, new[] { 1, 1 });

        Assert.Equal(307500m, result.Total);
    }

    [Fact]
    public void Option_not_offered_for_trim_throws()
    {
        var trim = CreateTrim();

        Assert.Throws<InvalidOperationException>(
            () => PriceCalculator.Calculate(trim, new[] { 99 }));
    }
}