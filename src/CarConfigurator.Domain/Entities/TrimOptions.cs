namespace CarConfigurator.Domain.Entities;

public class TrimOption
{
    public int TrimId { get; set; }
    public Trim Trim { get; set; } = null!;

    public int OptionId { get; set; }
    public Option Option { get; set; } = null!;

    public decimal? PriceOverride { get; set; }
}