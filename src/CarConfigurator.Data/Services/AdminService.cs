using System.ComponentModel.DataAnnotations;
using CarConfigurator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarConfigurator.Data.Services;

public class OptionFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(80, ErrorMessage = "Name can be at most 80 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(0, 1000000, ErrorMessage = "Price must be between 0 and 1,000,000.")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a category.")]
    public int OptionCategoryId { get; set; }

    public List<int> TrimIds { get; set; } = new();
}

public class AdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<OptionCategory>> GetCategoriesAsync()
    {
        return await _db.OptionCategories
            .OrderBy(c => c.SortOrder)
            .ToListAsync();
    }

    public async Task<List<Option>> GetOptionsAsync()
    {
        return await _db.Options
            .Include(o => o.OptionCategory)
            .OrderBy(o => o.OptionCategory.SortOrder)
            .ThenBy(o => o.Name)
            .ToListAsync();
    }

    public async Task<List<Trim>> GetTrimsAsync()
    {
        return await _db.Trims
            .Include(t => t.CarModel).ThenInclude(m => m.Brand)
            .OrderBy(t => t.CarModel.Brand.Name)
            .ThenBy(t => t.CarModel.Name)
            .ThenBy(t => t.BasePrice)
            .ToListAsync();
    }

    public async Task<OptionFormModel?> GetOptionFormAsync(int id)
    {
        var option = await _db.Options
            .Include(o => o.TrimOptions)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (option is null) return null;

        return new OptionFormModel
        {
            Id = option.Id,
            Name = option.Name,
            Price = option.Price,
            OptionCategoryId = option.OptionCategoryId,
            TrimIds = option.TrimOptions.Select(to => to.TrimId).ToList()
        };
    }

    public async Task SaveOptionAsync(OptionFormModel form)
    {
        Option option;

        if (form.Id == 0)
        {
            option = new Option();
            _db.Options.Add(option);
        }
        else
        {
            option = await _db.Options
                         .Include(o => o.TrimOptions)
                         .FirstOrDefaultAsync(o => o.Id == form.Id)
                     ?? throw new InvalidOperationException("Option not found.");
        }

        option.Name = form.Name.Trim();
        option.Price = form.Price;
        option.OptionCategoryId = form.OptionCategoryId;

        var wanted = form.TrimIds.ToHashSet();

        var toRemove = option.TrimOptions
            .Where(to => !wanted.Contains(to.TrimId))
            .ToList();
        _db.TrimOptions.RemoveRange(toRemove);

        var existing = option.TrimOptions.Select(to => to.TrimId).ToHashSet();
        foreach (var trimId in wanted.Where(id => !existing.Contains(id)))
            option.TrimOptions.Add(new TrimOption { TrimId = trimId });

        await _db.SaveChangesAsync();
    }
}