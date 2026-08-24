using ConsumerWorker.Models;
using Microsoft.EntityFrameworkCore;

namespace ConsumerWorker.Data;

public class IronGridDbContext : DbContext
{
    public IronGridDbContext(DbContextOptions<IronGridDbContext> options) : base(options)
    {
    }

    public DbSet<Unit> Units { get; set; } = null!;
    public DbSet<Asset> Assets { get; set; } = null!;
    public DbSet<AssetLiveStatus> AssetLiveStatuses { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unit -> Assets relationship (one-to-many)
        modelBuilder.Entity<Unit>()
            .HasMany(u => u.Assets)
            .WithOne(a => a.Unit)
            .HasForeignKey(a => a.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        // Asset -> AssetLiveStatus relationship (one-to-one)
        modelBuilder.Entity<Asset>()
            .HasOne(a => a.CurrentStatus)
            .WithOne(als => als.Asset)
            .HasForeignKey<AssetLiveStatus>(als => als.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index on AssetLiveStatus.AssetId
        modelBuilder.Entity<AssetLiveStatus>()
            .HasIndex(als => als.AssetId)
            .IsUnique();

        modelBuilder.Entity<Asset>()
            .Property(a => a.Type)
            .HasConversion<string>();

        modelBuilder.Entity<AssetLiveStatus>(entity =>
        {
            entity.Property(x => x.ProcessedStatus)
                .HasConversion<string>();
            entity.Property(x => x.IsVerified)
                .HasConversion<string>();
        });
    }
}
