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
        var criticalAssets = await (
            from asset in _context.Assets
            join status in _context.AssetLiveStatuses on asset.Id equals status.AssetId
            join unit in _context.Units on asset.UnitId equals unit.Id
            where status.ProcessedStatus == ProcessedStatus.Warning || status.IsVerified == false
            select new CriticalAssetDto
            {
                AssetId = asset.Id,
                AssetSerial = asset.AssetSerial,
                AssetType = asset.Type.ToString(),
                UnitName = unit.UnitName,
                Sector = unit.Sector,
                ProcessedStatus = status.ProcessedStatus.ToString(),
                IsVerified = status.IsVerified,
                LastUpdate = status.LastUpdate
            }
        ).ToListAsync();

        return Ok(criticalAssets);
    }

    [HttpGet("unit/{unitId}/assets")]
    public async Task<IActionResult> GetUnitAssets(int unitId)
    {
        var unitExists = await _context.Units.AnyAsync(u => u.Id == unitId);
        if (!unitExists)
            return NotFound();

        var unitAssets = await (
            from asset in _context.Assets
            join status in _context.AssetLiveStatuses on asset.Id equals status.AssetId into statusGroup
            from status in statusGroup.DefaultIfEmpty()
            where asset.UnitId == unitId
            select new UnitAssetDto
            {
                AssetId = asset.Id,
                AssetSerial = asset.AssetSerial,
                AssetType = asset.Type.ToString(),
                ProcessedStatus = status != null ? status.ProcessedStatus.ToString() : null,
                IsVerified = status != null ? status.IsVerified : (bool?)null,
                LastUpdate = status != null ? status.LastUpdate : (DateTime?)null
            }
        ).ToListAsync();

        return Ok(unitAssets);
    }

    [HttpGet("summary-by-unit")]
    public async Task<IActionResult> GetSummaryByUnit()
    {
        var summary = await (
            from unit in _context.Units
            join asset in _context.Assets on unit.Id equals asset.UnitId into assetGroup
            from asset in assetGroup.DefaultIfEmpty()
            join status in _context.AssetLiveStatuses on asset.Id equals status.AssetId into statusGroup
            from status in statusGroup.DefaultIfEmpty()
            group new { asset, status } by new { unit.Id, unit.UnitName, unit.Sector } into g
            select new UnitSummaryDto
            {
                UnitId = g.Key.Id,
                UnitName = g.Key.UnitName,
                Sector = g.Key.Sector,
                TotalAssets = g.Count(x => x.asset != null),
                StableAssets = g.Count(x => x.status != null && x.status.ProcessedStatus == ProcessedStatus.Stable),
                WarningAssets = g.Count(x => x.status != null && x.status.ProcessedStatus == ProcessedStatus.Warning),
                UnverifiedAssets = g.Count(x => x.status != null && x.status.IsVerified == false)
            }
        ).ToListAsync();

        return Ok(summary);
    }
}
