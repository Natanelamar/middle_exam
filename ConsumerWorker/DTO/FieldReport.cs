namespace ConsumerWorker.Dto;

public class FieldReport
{
    public int AssetId { get; set; }
    public string AssetType { get; set; } = string.Empty;
    public string RawValue { get; set; } = string.Empty;
    public string Timestamp { get; set; } = string.Empty;
}
