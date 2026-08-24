namespace ApiService.DTOs;

public class UnitAssetDto
{
    public int AssetId { get; set; }
    public string AssetSerial { get; set; } = string.Empty;
    public string AssetType { get; set; } = string.Empty;
    public string? ProcessedStatus { get; set; }
    public bool? IsVerified { get; set; }
    public DateTime? LastUpdate { get; set; }
}
