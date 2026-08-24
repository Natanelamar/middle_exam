using ApiService.DTOs;
using ApiService.Enums;
using ApiService.Repositories;
using Microsoft.AspNetCore.Mvc;
using ApiService.Services;

namespace ApiService.Controllers;

[ApiController]
[Route("api/assets-status")]
public class AssetsStatusController : ControllerBase
{
    private readonly AssetRepository _assetRepository;
    private readonly AssetLiveStatusRepository _statusRepository;
    private readonly RedisCacheService _cache;

    public AssetsStatusController(AssetRepository assetRepository, AssetLiveStatusRepository statusRepository, RedisCacheService cache)
    {
        _assetRepository = assetRepository;
        _statusRepository = statusRepository;
        _cache = cache;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAssetsStatus()
    {
        var allStatuses = await _statusRepository.GetAllAsync();
        var allDtos = allStatuses.Select(s => new AssetStatusDto
        {
            AssetId = s.AssetId,
            AssetSerial = s.Asset?.AssetSerial ?? "",
            AssetType = s.AssetType,
            UnitName = s.Asset?.Unit?.UnitName ?? "",
            Sector = s.Asset?.Unit?.Sector ?? "",
            RawValue = s.RawValue,
            ProcessedStatus = s.ProcessedStatus.ToString(),
            IsVerified = s.IsVerified,
            Asset = s.Asset == null ? null : new AssetDto
            {
                Id = s.Asset.Id,
                UnitId = s.Asset.UnitId,
                AssetSerial = s.Asset.AssetSerial,
                Type = s.Asset.Type.ToString()
            },
            LastUpdate = s.LastUpdate
        }).ToList();

        return Ok(allDtos);
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetAssetsStatusByStatus([FromQuery] string? status)
    {
        if (string.IsNullOrEmpty(status))
            return BadRequest("Status is required");

        if (!Enum.TryParse<ProcessedStatus>(status, true, out var parsedStatus))
            return BadRequest("Invalid status value");

        var filteredStatuses = await _statusRepository.GetByStatusAsync(parsedStatus);
        var filteredDtos = filteredStatuses.Select(s => new AssetStatusDto
        {
            AssetId = s.AssetId,
            AssetSerial = s.Asset?.AssetSerial ?? "",
            AssetType = s.AssetType,
            UnitName = s.Asset?.Unit?.UnitName ?? "",
            Sector = s.Asset?.Unit?.Sector ?? "",
            RawValue = s.RawValue,
            ProcessedStatus = s.ProcessedStatus.ToString(),
            IsVerified = s.IsVerified,
            Asset = s.Asset == null ? null : new AssetDto
            {
                Id = s.Asset.Id,
                UnitId = s.Asset.UnitId,
                AssetSerial = s.Asset.AssetSerial,
                Type = s.Asset.Type.ToString()
            },
            LastUpdate = s.LastUpdate
        }).ToList();

        return Ok(filteredDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetStatus(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");
        var cacheKey = $"asset-status-{id}";
        var cached = await _cache.GetAsync<AssetStatusDto>(cacheKey);
        if (cached != null)
            return Ok(cached);

        var status = await _statusRepository.GetByAssetIdAsync(id);
        if (status == null)
            return NotFound();

        var dto = new AssetStatusDto
        {
            AssetId = status.AssetId,
            AssetSerial = status.Asset?.AssetSerial ?? "",
            AssetType = status.AssetType,
            UnitName = status.Asset?.Unit?.UnitName ?? "",
            Sector = status.Asset?.Unit?.Sector ?? "",
            RawValue = status.RawValue,
            ProcessedStatus = status.ProcessedStatus.ToString(),
            IsVerified = status.IsVerified,
            Asset = status.Asset == null ? null : new AssetDto
            {
                Id = status.Asset.Id,
                UnitId = status.Asset.UnitId,
                AssetSerial = status.Asset.AssetSerial,
                Type = status.Asset.Type.ToString()
            },
            LastUpdate = status.LastUpdate
        };

        await _cache.SetAsync(cacheKey, dto);

        return Ok(dto);
    }
}
