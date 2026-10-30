using CarConfigurator.Domain.Entities;

namespace CarConfigurator.Domain.Pricing;

public record PriceLine(int OptionId, string Name, decimal Price);

public record PriceResult(decimal BasePrice, List<PriceLine> Lines, decimal Total);

public static class PriceCalculator
{
    public static PriceResult Calculate(Trim trim, IEnumerable<int> selectedOptionIds)
    {
        var lines = new List<PriceLine>();

        foreach (var optionId in selectedOptionIds.Distinct())
        {
            var trimOption = trim.TrimOptions.FirstOrDefault(to => to.OptionId == optionId);

            if (trimOption is null)
                throw new InvalidOperationException(
                    $"Option {optionId} is not offered for trim {trim.Name}.");

            var price = trimOption.PriceOverride ?? trimOption.Option.Price;
            lines.Add(new PriceLine(optionId, trimOption.Option.Name, price));
        }

        var total = trim.BasePrice + lines.Sum(l => l.Price);
        return new PriceResult(trim.BasePrice, lines, total);
    }
}