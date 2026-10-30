namespace CarConfigurator.Domain.Entities;

public class Trim
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public decimal BasePrice { get; set; }

    public int CarModelId { get; set; }
    public CarModel CarModel { get; set; } = null!;

    public List<TrimOption> TrimOptions { get; set; } = new();
}