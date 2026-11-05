using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MiniDpp.Api.Models;

namespace MiniDpp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProductModel> ProductModels => Set<ProductModel>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVersion> ProductVersions => Set<ProductVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // SQLite has no timezone-aware type; timestamps are stored as UTC and read back as UTC.
        var utc = new ValueConverter<DateTime, DateTime>(
            value => value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        modelBuilder.Entity<ProductModel>(entity =>
        {
            entity.HasKey(model => model.Id);
            entity.Property(model => model.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(model => model.Name).IsUnique();
        });

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(batch => batch.Id);
            entity.Property(batch => batch.BatchNumber).IsRequired().HasMaxLength(100);
            entity.Property(batch => batch.ProductionPlace).IsRequired().HasMaxLength(200);
            entity.HasIndex(batch => new { batch.ProductModelId, batch.BatchNumber }).IsUnique();
            entity.HasOne(batch => batch.ProductModel)
                .WithMany(model => model.Batches)
                .HasForeignKey(batch => batch.ProductModelId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(product => product.Gtin);
            entity.Property(product => product.Gtin).IsRequired().HasMaxLength(14);
            entity.Property(product => product.CottonPercentage).HasPrecision(5, 2);
            entity.Property(product => product.ElastanePercentage).HasPrecision(5, 2);
            entity.Property(product => product.CarbonKgCO2e).HasPrecision(12, 4);
            entity.Property(product => product.UpdatedAt).HasConversion(utc);
            entity.HasOne(product => product.Batch)
                .WithMany(batch => batch.Products)
                .HasForeignKey(product => product.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductVersion>(entity =>
        {
            entity.HasKey(version => version.Id);
            entity.Property(version => version.ProductGtin).IsRequired().HasMaxLength(14);
            entity.Property(version => version.ModelName).IsRequired().HasMaxLength(200);
            entity.Property(version => version.BatchNumber).IsRequired().HasMaxLength(100);
            entity.Property(version => version.ProductionPlace).IsRequired().HasMaxLength(200);
            entity.Property(version => version.CottonPercentage).HasPrecision(5, 2);
            entity.Property(version => version.ElastanePercentage).HasPrecision(5, 2);
            entity.Property(version => version.CarbonKgCO2e).HasPrecision(12, 4);
            entity.Property(version => version.RecordedAt).HasConversion(utc);
            entity.HasIndex(version => new { version.ProductGtin, version.VersionNumber }).IsUnique();
            entity.HasOne(version => version.Product)
                .WithMany(product => product.PreviousVersions)
                .HasForeignKey(version => version.ProductGtin)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
