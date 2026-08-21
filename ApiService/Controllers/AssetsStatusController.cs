using ApiService.DTOs;
using ApiService.Enums;
using ApiService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/assets-status")]
public class AssetsStatusController : ControllerBase
{
    private readonly AssetRepository _assetRepository;
    private readonly AssetLiveStatusRepository _statusRepository;

    public AssetsStatusController(AssetRepository assetRepository, AssetLiveStatusRepository statusRepository)
    {
        _assetRepository = assetRepository;
        _statusRepository = statusRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAssetsStatus([FromQuery] string? status)
    {
        if (!string.IsNullOrEmpty(status))
        {
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
                LastUpdate = s.LastUpdate
            }).ToList();

            return Ok(filteredDtos);
        }

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
            LastUpdate = s.LastUpdate
        }).ToList();

        return Ok(allDtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAssetStatus(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

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
            LastUpdate = status.LastUpdate
        };

        return Ok(dto);
    }
}
