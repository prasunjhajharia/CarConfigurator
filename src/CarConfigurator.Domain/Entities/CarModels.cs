namespace CarConfigurator.Domain.Entities;

public class CarModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public int BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    public List<Trim> Trims { get; set; } = new();
}