namespace CarConfigurator.Domain.Entities;

public enum RuleType
{
    Requires,
    Excludes
}

public class OptionRule
{
    public int Id { get; set; }

    public int OptionId { get; set; }
    public Option Option { get; set; } = null!;

    public int RelatedOptionId { get; set; }
    public Option RelatedOption { get; set; } = null!;

    public RuleType RuleType { get; set; }
}