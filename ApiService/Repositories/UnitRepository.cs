using ApiService.Data;
using ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Repositories;

public class UnitRepository : IRepository<Unit>
{
    private readonly IronGridDbContext _context;

    public UnitRepository(IronGridDbContext context)
    {
        _context = context;
    }

    public async Task<Unit?> GetByIdAsync(int id)
    {
        return await _context.Units
            .Include(u => u.Assets)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Unit>> GetAllAsync()
    {
        return await _context.Units
            .Include(u => u.Assets)
            .ToListAsync();
    }

    public async Task<Unit> AddAsync(Unit entity)
    {
        _context.Units.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Unit> UpdateAsync(Unit entity)
    {
        _context.Units.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var unit = await _context.Units.FindAsync(id);
        if (unit == null)
            return false;

        _context.Units.Remove(unit);
        await _context.SaveChangesAsync();
        return true;
    }
}
