using ApiService.DTOs;
using ApiService.Models;
using ApiService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApiService.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetsController : ControllerBase
{
    private readonly AssetRepository _assetRepository;
    private readonly UnitRepository _unitRepository;

    public AssetsController(AssetRepository assetRepository, UnitRepository unitRepository)
    {
        _assetRepository = assetRepository;
        _unitRepository = unitRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsset(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

        var asset = await _assetRepository.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        var dto = new AssetDto
        {
            Id = asset.Id,
            UnitId = asset.UnitId,
            AssetSerial = asset.AssetSerial,
            Type = asset.Type.ToString()
        };

        return Ok(dto);
    }

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitDto unitDto)
    {
        if (string.IsNullOrWhiteSpace(unitDto.UnitName))
            return BadRequest("UnitName is required");
        
        var unit = new Unit
        {
            UnitName = unitDto.UnitName,
            Sector = unitDto.Sector
        };

        await _unitRepository.AddAsync(unit);
        return StatusCode(201);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAsset(int id, [FromBody] Asset asset)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

        if (asset == null)
            return BadRequest("Asset is required");

        if (string.IsNullOrWhiteSpace(asset.AssetSerial))
            return BadRequest("AssetSerial is required");

        var existing = await _assetRepository.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        asset.Id = id;
        var updated = await _assetRepository.UpdateAsync(asset);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsset(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid Id");

        var result = await _assetRepository.DeleteAsync(id);
        if (!result)
            return NotFound();

        return NoContent();
    }
}
