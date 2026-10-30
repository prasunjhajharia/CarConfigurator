using System.ComponentModel.DataAnnotations;
using CarConfigurator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarConfigurator.Data.Services;

public class TrimFormModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(60, ErrorMessage = "Name can be at most 60 characters.")]
    public string Name { get; set; } = string.Empty;

    [Range(1, 10000000, ErrorMessage = "Base price must be between 1 and 10,000,000.")]
    public decimal BasePrice { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Choose a model.")]
    public int CarModelId { get; set; }

    public int? CopyOptionsFromTrimId { get; set; }
}

public class TrimAdminService
{
    private readonly AppDbContext _db;

    public TrimAdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<CarModel>> GetModelsAsync()
    {
        return await _db.CarModels
            .Include(m => m.Brand)
            .OrderBy(m => m.Brand.Name)
            .ThenBy(m => m.Name)
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

    public async Task<TrimFormModel?> GetTrimFormAsync(int id)
    {
        var trim = await _db.Trims.FindAsync(id);
        if (trim is null) return null;

        return new TrimFormModel
        {
            Id = trim.Id,
            Name = trim.Name,
            BasePrice = trim.BasePrice,
            CarModelId = trim.CarModelId
        };
    }

    public async Task SaveTrimAsync(TrimFormModel form)
    {
        Trim trim;

        if (form.Id == 0)
        {
            trim = new Trim();
            _db.Trims.Add(trim);

            if (form.CopyOptionsFromTrimId is int sourceId)
            {
                var source = await _db.TrimOptions
                    .Where(to => to.TrimId == sourceId)
                    .ToListAsync();

                foreach (var link in source)
                {
                    trim.TrimOptions.Add(new TrimOption
                    {
                        OptionId = link.OptionId,
                        PriceOverride = link.PriceOverride
                    });
                }
            }
        }
        else
        {
            trim = await _db.Trims.FindAsync(form.Id)
                   ?? throw new InvalidOperationException("Trim not found.");
        }

        trim.Name = form.Name.Trim();
        trim.BasePrice = form.BasePrice;
        trim.CarModelId = form.CarModelId;

        await _db.SaveChangesAsync();
    }
}