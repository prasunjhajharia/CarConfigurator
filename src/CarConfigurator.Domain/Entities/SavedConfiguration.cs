namespace CarConfigurator.Domain.Entities;

public class SavedConfiguration
{
    public int Id { get; set; }
    public Guid ShareCode { get; set; } = Guid.NewGuid();
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TrimId { get; set; }
    public Trim Trim { get; set; } = null!;

    public List<SavedConfigurationItem> Items { get; set; } = new();
}