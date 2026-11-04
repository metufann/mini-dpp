using Microsoft.EntityFrameworkCore;
using MiniDpp.Api.Models;

namespace MiniDpp.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ProductModel> ProductModels => Set<ProductModel>();
    public DbSet<Batch> Batches => Set<Batch>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
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
            entity.HasOne(product => product.Batch)
                .WithMany(batch => batch.Products)
                .HasForeignKey(product => product.BatchId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
