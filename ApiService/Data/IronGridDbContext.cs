using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Data;

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

        modelBuilder.Entity<Asset>()
            .HasOne(a => a.Unit)
            .WithMany(u => u.Assets)
            .HasForeignKey(a => a.UnitId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetLiveStatus>()
            .HasOne(als => als.Asset)
            .WithOne(a => a.CurrentStatus)
            .HasForeignKey<AssetLiveStatus>(als => als.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AssetLiveStatus>()
            .HasIndex(als => als.AssetId)
            .IsUnique();
    }
}
