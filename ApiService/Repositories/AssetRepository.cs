using ApiService.Data;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Repositories;

public class AssetRepository : IRepository<Asset>
{
    private readonly IronGridDbContext _context;

    public AssetRepository(IronGridDbContext context)
    {
        _context = context;
    }

    public async Task<Asset?> GetByIdAsync(int id)
    {
        return await _context.Assets
            .Include(a => a.Unit)
            .Include(a => a.CurrentStatus)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await _context.Assets
            .Include(a => a.Unit)
            .Include(a => a.CurrentStatus)
            .ToListAsync();
    }

    public async Task<Asset> AddAsync(Asset entity)
    {
        _context.Assets.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Asset> UpdateAsync(Asset entity)
    {
        _context.Assets.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null)
            return false;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }
}
