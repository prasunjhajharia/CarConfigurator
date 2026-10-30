using CarConfigurator.Data;
using CarConfigurator.Data.Services;
using CarConfigurator.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CarConfigurator.Tests;

public class ConfiguratorServiceTests
{
    private class TestData
    {
        public AppDbContext Db { get; init; } = null!;
        public ConfiguratorService Service { get; init; } = null!;
        public int TrimId { get; init; }
        public int SportId { get; init; }
        public int AlloyId { get; init; }
        public int DieselId { get; init; }
    }

    private static async Task<TestData> SetupAsync()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var db = new AppDbContext(options);

        var packages = new OptionCategory { Name = "Packages", SortOrder = 5 };
        var wheels = new OptionCategory { Name = "Wheels", SortOrder = 3 };
        var engine = new OptionCategory { Name = "Engine", SortOrder = 1 };

        var sport = new Option { Name = "Sport package", Price = 18000m, OptionCategory = packages };
        var alloy = new Option { Name = "17-inch alloy", Price = 5000m, OptionCategory = wheels };
        var diesel = new Option { Name = "2.0 Diesel", Price = 25000m, OptionCategory = engine };

        var trim = new Trim
        {
            Name = "Style",
            BasePrice = 300000m,
            CarModel = new CarModel { Name = "Golf", Brand = new Brand { Name = "Volkswagen" } }
        };

        trim.TrimOptions.Add(new TrimOption { Option = sport });
        trim.TrimOptions.Add(new TrimOption { Option = alloy });
        trim.TrimOptions.Add(new TrimOption { Option = diesel });

        db.Trims.Add(trim);
        await db.SaveChangesAsync();

        db.OptionRules.AddRange(
            new OptionRule { OptionId = sport.Id, RelatedOptionId = alloy.Id, RuleType = RuleType.Requires },
            new OptionRule { OptionId = sport.Id, RelatedOptionId = diesel.Id, RuleType = RuleType.Excludes });
        await db.SaveChangesAsync();

        return new TestData
        {
            Db = db,
            Service = new ConfiguratorService(db),
            TrimId = trim.Id,
            SportId = sport.Id,
            AlloyId = alloy.Id,
            DieselId = diesel.Id
        };
    }

    [Fact]
    public async Task CalculatePrice_returns_total_for_valid_selection()
    {
        var data = await SetupAsync();

        var result = await data.Service.CalculatePriceAsync(data.TrimId, new[] { data.SportId, data.AlloyId });

        Assert.NotNull(result);
        Assert.Equal(323000m, result.Total);
    }

    [Fact]
    public async Task CalculatePrice_throws_when_required_option_is_missing()
    {
        var data = await SetupAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => data.Service.CalculatePriceAsync(data.TrimId, new[] { data.SportId }));
    }

    [Fact]
    public async Task CalculatePrice_throws_when_excluded_options_are_selected()
    {
        var data = await SetupAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => data.Service.CalculatePriceAsync(data.TrimId, new[] { data.SportId, data.AlloyId, data.DieselId }));
    }

    [Fact]
    public async Task CalculatePrice_returns_null_for_unknown_trim()
    {
        var data = await SetupAsync();

        var result = await data.Service.CalculatePriceAsync(9999, new[] { data.AlloyId });

        Assert.Null(result);
    }

    [Fact]
    public async Task Save_stores_receipt_with_prices_at_save_time()
    {
        var data = await SetupAsync();

        var id = await data.Service.SaveConfigurationAsync(data.TrimId, new[] { data.AlloyId });

        Assert.NotNull(id);

        var saved = await data.Db.SavedConfigurations
            .Include(s => s.Items)
            .SingleAsync();

        Assert.Equal(id.Value, saved.ShareCode);
        Assert.Equal(305000m, saved.TotalPrice);

        var item = Assert.Single(saved.Items);
        Assert.Equal(5000m, item.PriceAtSave);
    }

    [Fact]
    public async Task Save_stores_nothing_when_rules_are_broken()
    {
        var data = await SetupAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => data.Service.SaveConfigurationAsync(data.TrimId, new[] { data.SportId }));

        Assert.Equal(0, await data.Db.SavedConfigurations.CountAsync());
    }
}