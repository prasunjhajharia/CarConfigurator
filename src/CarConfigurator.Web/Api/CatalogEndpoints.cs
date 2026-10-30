using CarConfigurator.Data.Services;

namespace CarConfigurator.Web.Api;

public record PriceRequest(int TrimId, List<int>? OptionIds);

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api");

        api.MapGet("/brands", async (ConfiguratorService service) =>
        {
            var brands = await service.GetBrandsAsync();

            return Results.Ok(brands.Select(b => new
            {
                b.Id,
                b.Name,
                Models = b.CarModels.Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.ImageUrl,
                    FromPrice = m.Trims.Count == 0 ? (decimal?)null : m.Trims.Min(t => t.BasePrice)
                })
            }));
        });

        api.MapGet("/models/{id:int}/trims", async (int id, ConfiguratorService service) =>
        {
            var model = await service.GetModelWithTrimsAsync(id);
            if (model is null) return Results.NotFound();

            return Results.Ok(new
            {
                model.Id,
                model.Name,
                Brand = model.Brand.Name,
                Trims = model.Trims.Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.BasePrice,
                    ImageUrl = t.ImageUrl ?? model.ImageUrl
                })
            });
        });

        api.MapGet("/trims/{id:int}/config", async (int id, ConfiguratorService service) =>
        {
            var config = await service.GetTrimConfigAsync(id);
            if (config is null) return Results.NotFound();

            return Results.Ok(new
            {
                config.TrimId,
                config.BrandName,
                config.ModelName,
                config.TrimName,
                config.BasePrice,
                config.Categories,
                Rules = config.Rules.Select(r => new
                {
                    r.OptionId,
                    r.RelatedOptionId,
                    RuleType = r.RuleType.ToString()
                })
            });
        });

        api.MapPost("/price", async (PriceRequest request, ConfiguratorService service) =>
        {
            try
            {
                var result = await service.CalculatePriceAsync(
                    request.TrimId, request.OptionIds ?? new List<int>());

                return result is null ? Results.NotFound() : Results.Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}