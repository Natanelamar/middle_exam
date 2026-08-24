using ApiService.Data;
using ApiService.Enums;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Repositories;

public class AssetLiveStatusRepository : IRepository<AssetLiveStatus>
{
    private readonly IronGridDbContext _context;

    public AssetLiveStatusRepository(IronGridDbContext context)
    {
        _context = context;
    }

    public async Task<AssetLiveStatus?> GetByIdAsync(int id)
    {
        return await _context.AssetLiveStatuses
            .Include(als => als.Asset)
            .ThenInclude(a => a!.Unit)
            .FirstOrDefaultAsync(als => als.Id == id);
    }

    public async Task<IEnumerable<AssetLiveStatus>> GetAllAsync()
    {
        return await _context.AssetLiveStatuses
            .Include(als => als.Asset)
            .ThenInclude(a => a!.Unit)
            .ToListAsync();
    }

    public async Task<AssetLiveStatus?> GetByAssetIdAsync(int assetId)
    {
        return await _context.AssetLiveStatuses
            .Include(als => als.Asset)
            .ThenInclude(a => a!.Unit)
            .FirstOrDefaultAsync(als => als.AssetId == assetId);
    }

    public async Task<IEnumerable<AssetLiveStatus>> GetByStatusAsync(ProcessedStatus status)
    {
        return await _context.AssetLiveStatuses
            .Include(als => als.Asset)
            .ThenInclude(a => a!.Unit)
            .Where(als => als.ProcessedStatus == status)
            .ToListAsync();
    }

    public async Task<AssetLiveStatus> AddAsync(AssetLiveStatus entity)
    {
        _context.AssetLiveStatuses.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<AssetLiveStatus> UpdateAsync(AssetLiveStatus entity)
    {
        _context.AssetLiveStatuses.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var status = await _context.AssetLiveStatuses.FindAsync(id);
        if (status == null)
            return false;

        _context.AssetLiveStatuses.Remove(status);
        await _context.SaveChangesAsync();
        return true;
    }
}
