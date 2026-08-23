using ApiService.Data;
using ApiService.DTOs;
using ApiService.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiService.Controllers;

[ApiController]
[Route("api/reports")]
public class OperationsReportsController : ControllerBase
{
    private readonly IronGridDbContext _context;

    public OperationsReportsController(IronGridDbContext context)
    {
        _context = context;
    }

    [HttpGet("critical-assets")]
    public async Task<IActionResult> GetCriticalAssets()
    {
        var criticalAssets = await _context.Assets
            .AsNoTracking()
            .Include(a => a.Unit)
            .Include(a => a.CurrentStatus)
            .Where(a => a.CurrentStatus != null &&
                        (a.CurrentStatus.ProcessedStatus == ProcessedStatus.Warning ||
                         !a.CurrentStatus.IsVerified))
            .Select(a => new CriticalAssetDto
            {
                AssetId = a.Id,
                AssetSerial = a.AssetSerial,
                AssetType = a.Type.ToString(),
                UnitName = a.Unit != null ? a.Unit.UnitName : "",
                Sector = a.Unit != null ? a.Unit.Sector : "",
                ProcessedStatus = a.CurrentStatus!.ProcessedStatus.ToString(),
                IsVerified = a.CurrentStatus!.IsVerified,
                LastUpdate = a.CurrentStatus!.LastUpdate
            })
            .ToListAsync();

        return Ok(criticalAssets);
    }

    [HttpGet("unit/{unitId}/assets")]
    public async Task<IActionResult> GetUnitAssets(int unitId)
    {
        var unitExists = await _context.Units.AnyAsync(u => u.Id == unitId);
        if (!unitExists)
            return NotFound();

        var unitAssets = await _context.Assets
            .AsNoTracking()
            .Where(a => a.UnitId == unitId)
            .Include(a => a.CurrentStatus)
            .Select(a => new UnitAssetDto
            {
                AssetId = a.Id,
                AssetSerial = a.AssetSerial,
                AssetType = a.Type.ToString(),
                ProcessedStatus = a.CurrentStatus != null ? a.CurrentStatus.ProcessedStatus.ToString() : null,
                IsVerified = a.CurrentStatus != null ? a.CurrentStatus.IsVerified : null,
                LastUpdate = a.CurrentStatus != null ? a.CurrentStatus.LastUpdate : null
            })
            .ToListAsync();

        return Ok(unitAssets);
    }

    [HttpGet("summary-by-unit")]
    public async Task<IActionResult> GetSummaryByUnit()
    {
        var summary = await _context.Units
            .AsNoTracking()
            .Include(u => u.Assets)
                .ThenInclude(a => a.CurrentStatus)
            .Select(u => new UnitSummaryDto
            {
                UnitId = u.Id,
                UnitName = u.UnitName,
                Sector = u.Sector,
                TotalAssets = u.Assets.Count,
                StableAssets = u.Assets.Count(a => a.CurrentStatus != null && a.CurrentStatus.ProcessedStatus == ProcessedStatus.Stable),
                WarningAssets = u.Assets.Count(a => a.CurrentStatus != null && a.CurrentStatus.ProcessedStatus == ProcessedStatus.Warning),
                UnverifiedAssets = u.Assets.Count(a => a.CurrentStatus != null && !a.CurrentStatus.IsVerified)
            })
            .ToListAsync();

        return Ok(summary);
    }
}
