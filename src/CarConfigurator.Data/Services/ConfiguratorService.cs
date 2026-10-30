using CarConfigurator.Domain.Entities;
using CarConfigurator.Domain.Pricing;
using Microsoft.EntityFrameworkCore;
using CarConfigurator.Domain.Rules;

namespace CarConfigurator.Data.Services;

public record OptionDto(int Id, string Name, decimal Price, string? ImageUrl);

public record CategoryDto(int Id, string Name, List<OptionDto> Options);

public record TrimConfigDto(
    int TrimId,
    string BrandName,
    string ModelName,
    string TrimName,
    decimal BasePrice,
    List<CategoryDto> Categories,
    List<OptionRule> Rules,
    string? ImageUrl);

public class ConfiguratorService
{
    private readonly AppDbContext _db;

    public ConfiguratorService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Brand>> GetBrandsAsync()
    {
        return await _db.Brands
            .Include(b => b.CarModels).ThenInclude(m => m.Trims)
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<CarModel?> GetModelWithTrimsAsync(int modelId)
    {
        return await _db.CarModels
            .Include(m => m.Brand)
            .Include(m => m.Trims.OrderBy(t => t.BasePrice))
            .FirstOrDefaultAsync(m => m.Id == modelId);
    }

    public async Task<TrimConfigDto?> GetTrimConfigAsync(int trimId)
    {
        var trim = await _db.Trims
            .Include(t => t.CarModel).ThenInclude(m => m.Brand)
            .Include(t => t.TrimOptions).ThenInclude(to => to.Option).ThenInclude(o => o.OptionCategory)
            .FirstOrDefaultAsync(t => t.Id == trimId);

        if (trim is null) return null;

        var categories = trim.TrimOptions
            .GroupBy(to => to.Option.OptionCategoryId)
            .Select(g => new
            {
                Category = g.First().Option.OptionCategory,
                Options = g.Select(to => new OptionDto(
                        to.OptionId,
                        to.Option.Name,
                        to.PriceOverride ?? to.Option.Price,
                        to.Option.ImageUrl))
                    .OrderBy(o => o.Price)
                    .ToList()
            })
            .OrderBy(x => x.Category.SortOrder)
            .Select(x => new CategoryDto(x.Category.Id, x.Category.Name, x.Options))
            .ToList();

        var offeredIds = trim.TrimOptions.Select(to => to.OptionId).ToHashSet();

        var rules = (await _db.OptionRules.ToListAsync())
            .Where(r => offeredIds.Contains(r.OptionId) && offeredIds.Contains(r.RelatedOptionId))
            .ToList();

            return new TrimConfigDto(
            trim.Id,
            trim.CarModel.Brand.Name,
            trim.CarModel.Name,
            trim.Name,
            trim.BasePrice,
            categories,
            rules,
            trim.ImageUrl ?? trim.CarModel.ImageUrl);
    }

    public async Task<PriceResult?> CalculatePriceAsync(int trimId, IEnumerable<int> selectedOptionIds)
    {
        var trim = await _db.Trims
            .Include(t => t.TrimOptions).ThenInclude(to => to.Option)
            .FirstOrDefaultAsync(t => t.Id == trimId);

        if (trim is null) return null;

        
        await EnsureRulesRespectedAsync(trim, selectedOptionIds);
        return PriceCalculator.Calculate(trim, selectedOptionIds);
    }
        public async Task<Guid?> SaveConfigurationAsync(int trimId, IEnumerable<int> selectedOptionIds)
    {
        var trim = await _db.Trims
            .Include(t => t.TrimOptions).ThenInclude(to => to.Option)
            .FirstOrDefaultAsync(t => t.Id == trimId);

        if (trim is null) return null;

        
        await EnsureRulesRespectedAsync(trim, selectedOptionIds);
        var result = PriceCalculator.Calculate(trim, selectedOptionIds);
        var saved = new SavedConfiguration
        {
            TrimId = trimId,
            TotalPrice = result.Total,
            Items = result.Lines.Select(l => new SavedConfigurationItem
            {
                OptionId = l.OptionId,
                PriceAtSave = l.Price
            }).ToList()
        };

        _db.SavedConfigurations.Add(saved);
        await _db.SaveChangesAsync();

        return saved.ShareCode;
    }

    public async Task<SavedConfiguration?> GetSavedConfigurationAsync(Guid code)
    {
        return await _db.SavedConfigurations
            .Include(s => s.Trim).ThenInclude(t => t.CarModel).ThenInclude(m => m.Brand)
            .Include(s => s.Items).ThenInclude(i => i.Option).ThenInclude(o => o.OptionCategory)
            .FirstOrDefaultAsync(s => s.ShareCode == code);
    }
        public async Task<List<SavedConfiguration>> GetSavedConfigurationsAsync()
    {
        return await _db.SavedConfigurations
            .Include(s => s.Trim).ThenInclude(t => t.CarModel).ThenInclude(m => m.Brand)
            .OrderByDescending(s => s.CreatedAt)
            .Take(50)
            .ToListAsync();
    }
        private async Task EnsureRulesRespectedAsync(Trim trim, IEnumerable<int> selectedOptionIds)
    {
        var offeredIds = trim.TrimOptions.Select(to => to.OptionId).ToHashSet();

        var rules = (await _db.OptionRules.ToListAsync())
            .Where(r => offeredIds.Contains(r.OptionId) && offeredIds.Contains(r.RelatedOptionId))
            .ToList();

        var violations = RuleEngine.FindViolations(selectedOptionIds.Distinct(), rules);

        if (violations.Count > 0)
            throw new InvalidOperationException(
                "The selected options break one or more compatibility rules.");
    }
}