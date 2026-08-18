namespace ProducerService.Models;

public class Asset
{
    public int Id { get; set; }
    public int UnitId { get; set; }
    public string AssetSerial { get; set; } = string.Empty;
    public string AssetType { get; set;} = string.Empty;
}

