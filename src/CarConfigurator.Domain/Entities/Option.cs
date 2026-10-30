namespace CarConfigurator.Domain.Entities;

public class Option
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public int OptionCategoryId { get; set; }
    public OptionCategory OptionCategory { get; set; } = null!;

    public List<TrimOption> TrimOptions { get; set; } = new();
}