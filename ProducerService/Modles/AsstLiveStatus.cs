using ProducerService.Models;

public class AssetLiveStatus
{
    public int AssetId { get; set;}
    public string AssetType { get; set; } = string.Empty;

    public string RawValue { get; set; } = string.Empty;

    public string ProcessedStatus { get; set;} = string.Empty;

    public bool IsVerfied { get; set; }
    public DateTime LastUpdate { get; set;}
}