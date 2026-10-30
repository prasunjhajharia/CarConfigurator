namespace CarConfigurator.Domain.Entities;

public class OptionCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public List<Option> Options { get; set; } = new();
}