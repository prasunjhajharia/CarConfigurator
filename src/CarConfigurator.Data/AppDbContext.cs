using CarConfigurator.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarConfigurator.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<CarModel> CarModels => Set<CarModel>();
    public DbSet<Trim> Trims => Set<Trim>();
    public DbSet<OptionCategory> OptionCategories => Set<OptionCategory>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<TrimOption> TrimOptions => Set<TrimOption>();
    public DbSet<OptionRule> OptionRules => Set<OptionRule>();
    public DbSet<SavedConfiguration> SavedConfigurations => Set<SavedConfiguration>();
    public DbSet<SavedConfigurationItem> SavedConfigurationItems => Set<SavedConfigurationItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TrimOption>()
            .HasKey(to => new { to.TrimId, to.OptionId });

        modelBuilder.Entity<SavedConfiguration>()
            .HasIndex(s => s.ShareCode)
            .IsUnique();
    }
}