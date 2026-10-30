using CarConfigurator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarConfigurator.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Brands.AnyAsync()) return;

        var volkswagen = new Brand
        {
            Name = "Volkswagen",
            CarModels =
            {
                new CarModel
                {
                    Name = "Golf",
                    Trims =
                    {
                        new Trim { Name = "Life", BasePrice = 289000m },
                        new Trim { Name = "Style", BasePrice = 329000m },
                        new Trim { Name = "R-Line", BasePrice = 369000m }
                    }
                },
                new CarModel
                {
                    Name = "Polo",
                    Trims =
                    {
                        new Trim { Name = "Life", BasePrice = 219000m },
                        new Trim { Name = "Style", BasePrice = 249000m }
                    }
                }
            }
        };

        var toyota = new Brand
        {
            Name = "Toyota",
            CarModels =
            {
                new CarModel
                {
                    Name = "Yaris",
                    Trims =
                    {
                        new Trim { Name = "Active", BasePrice = 199000m },
                        new Trim { Name = "Comfort", BasePrice = 229000m }
                    }
                },
                new CarModel
                {
                    Name = "Corolla",
                    Trims =
                    {
                        new Trim { Name = "Active", BasePrice = 259000m },
                        new Trim { Name = "Executive", BasePrice = 309000m }
                    }
                }
            }
        };

        db.Brands.AddRange(volkswagen, toyota);
        await db.SaveChangesAsync();
    }
        public static async Task SeedOptionsAsync(AppDbContext db)
    {
        if (await db.OptionCategories.AnyAsync()) return;

        var categories = new List<OptionCategory>
        {
            new OptionCategory
            {
                Name = "Engine", SortOrder = 1,
                Options =
                {
                    new Option { Name = "1.0 Petrol", Price = 0m },
                    new Option { Name = "1.5 Petrol", Price = 15000m },
                    new Option { Name = "2.0 Diesel", Price = 25000m },
                    new Option { Name = "Hybrid", Price = 32000m }
                }
            },
            new OptionCategory
            {
                Name = "Color", SortOrder = 2,
                Options =
                {
                    new Option { Name = "White", Price = 0m },
                    new Option { Name = "Black", Price = 6000m },
                    new Option { Name = "Red", Price = 7500m },
                    new Option { Name = "Blue", Price = 7500m }
                }
            },
            new OptionCategory
            {
                Name = "Wheels", SortOrder = 3,
                Options =
                {
                    new Option { Name = "16-inch steel", Price = 0m },
                    new Option { Name = "17-inch alloy", Price = 5000m },
                    new Option { Name = "18-inch alloy", Price = 9000m }
                }
            },
            new OptionCategory
            {
                Name = "Interior", SortOrder = 4,
                Options =
                {
                    new Option { Name = "Cloth seats", Price = 0m },
                    new Option { Name = "Leather seats", Price = 14000m }
                }
            },
            new OptionCategory
            {
                Name = "Packages", SortOrder = 5,
                Options =
                {
                    new Option { Name = "Winter package", Price = 7000m },
                    new Option { Name = "Technology package", Price = 12000m },
                    new Option { Name = "Sport package", Price = 18000m }
                }
            }
        };

        db.OptionCategories.AddRange(categories);
        await db.SaveChangesAsync();
    }
        public static async Task SeedTrimOptionsAsync(AppDbContext db)
    {
        if (await db.TrimOptions.AnyAsync()) return;

        var options = await db.Options.ToListAsync();
        var trims = await db.Trims
            .Include(t => t.CarModel)
            .ThenInclude(m => m.Brand)
            .ToListAsync();

        foreach (var trim in trims)
        {
            var model = trim.CarModel.Name;
            var brand = trim.CarModel.Brand.Name;
            var isSmallCar = model is "Polo" or "Yaris";
            var isEntryTrim = trim.Name is "Life" or "Active";

            foreach (var option in options)
            {
                var offered = option.Name switch
                {
                    "2.0 Diesel" => model is "Golf" or "Corolla",
                    "Hybrid" => brand == "Toyota",
                    "18-inch alloy" => !isSmallCar,
                    "Leather seats" => !isEntryTrim,
                    "Sport package" => trim.Name is "Style" or "R-Line",
                    _ => true
                };

                if (!offered) continue;

                decimal? priceOverride =
                    option.Name == "Leather seats" && (trim.Name is "R-Line" or "Executive")
                        ? 0m
                        : null;

                db.TrimOptions.Add(new TrimOption
                {
                    TrimId = trim.Id,
                    OptionId = option.Id,
                    PriceOverride = priceOverride
                });
            }
        }

        await db.SaveChangesAsync();
    }
        public static async Task SeedRulesAsync(AppDbContext db)
    {
        if (await db.OptionRules.AnyAsync()) return;

        var options = await db.Options.ToDictionaryAsync(o => o.Name);

        db.OptionRules.AddRange(
            new OptionRule
            {
                OptionId = options["Sport package"].Id,
                RelatedOptionId = options["17-inch alloy"].Id,
                RuleType = RuleType.Requires
            },
            new OptionRule
            {
                OptionId = options["Sport package"].Id,
                RelatedOptionId = options["2.0 Diesel"].Id,
                RuleType = RuleType.Excludes
            },
            new OptionRule
            {
                OptionId = options["Technology package"].Id,
                RelatedOptionId = options["17-inch alloy"].Id,
                RuleType = RuleType.Requires
            }
        );

        await db.SaveChangesAsync();
    }
        public static async Task SeedModelImagesAsync(AppDbContext db)
    {
        var images = new Dictionary<string, string>
        {
            ["Golf"] = "/images/models/golf.jpg",
            ["Polo"] = "/images/models/polo.jpg",
            ["Yaris"] = "/images/models/yaris.jpg",
            ["Corolla"] = "/images/models/corolla.jpg"
        };

        var models = await db.CarModels.Where(m => m.ImageUrl == null).ToListAsync();

        foreach (var model in models)
        {
            if (images.TryGetValue(model.Name, out var path))
                model.ImageUrl = path;
        }

        await db.SaveChangesAsync();
    }
        public static async Task SeedTrimImagesAsync(AppDbContext db)
    {
        var images = new Dictionary<string, string>
        {
            ["Golf Life"] = "/images/trims/golf-life.jpg",
            ["Golf Style"] = "/images/trims/golf-style.jpg",
            ["Golf R-Line"] = "/images/trims/golf-rline.jpg",
            ["Polo Life"] = "/images/trims/polo-life.jpg",
            ["Polo Style"] = "/images/trims/polo-style.jpg",
            ["Yaris Active"] = "/images/trims/yaris-active.jpg",
            ["Yaris Comfort"] = "/images/trims/yaris-comfort.jpg",
            ["Corolla Active"] = "/images/trims/corolla-active.jpg",
            ["Corolla Executive"] = "/images/trims/corolla-executive.jpg"
        };

        var trims = await db.Trims
            .Include(t => t.CarModel)
            .Where(t => t.ImageUrl == null)
            .ToListAsync();

        foreach (var trim in trims)
        {
            var key = $"{trim.CarModel.Name} {trim.Name}";
            if (images.TryGetValue(key, out var path))
                trim.ImageUrl = path;
        }

        await db.SaveChangesAsync();
    }
    
}