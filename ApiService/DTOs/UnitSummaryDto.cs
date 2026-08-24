namespace ApiService.DTOs;

public class UnitSummaryDto
{
    public int UnitId { get; set; }
    public string UnitName { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public int TotalAssets { get; set; }
    public int StableAssets { get; set; }
    public int WarningAssets { get; set; }
    public int UnverifiedAssets { get; set; }
}
