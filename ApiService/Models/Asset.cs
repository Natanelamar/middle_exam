using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiService.Enums;

namespace ApiService.Models;

public class Asset
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UnitId { get; set; }

    [Required]
    public string AssetSerial { get; set; } = string.Empty;

    [Required]
    [DefaultValue(AssetType.GenericAsset)]
    public AssetType Type { get; set; } = AssetType.GenericAsset;

    [ForeignKey(nameof(UnitId))]
    public Unit? Unit { get; set; }

    public AssetLiveStatus? CurrentStatus { get; set; }
}
