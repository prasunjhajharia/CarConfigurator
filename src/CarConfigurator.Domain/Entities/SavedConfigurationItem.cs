namespace CarConfigurator.Domain.Entities;

public class SavedConfigurationItem
{
    public int Id { get; set; }
    public decimal PriceAtSave { get; set; }

    public int SavedConfigurationId { get; set; }
    public SavedConfiguration SavedConfiguration { get; set; } = null!;

    public int OptionId { get; set; }
    public Option Option { get; set; } = null!;
}