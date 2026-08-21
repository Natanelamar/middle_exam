using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ApiService.Enums;

namespace ApiService.Models;

public class AssetLiveStatus
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int AssetId { get; set; }

    [Required]
    [MaxLength(50)]
    public string AssetType { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string RawValue { get; set; } = string.Empty;

    [Required]
    public ProcessedStatus ProcessedStatus { get; set; }

    [Required]
    [DefaultValue(false)]
    public bool IsVerified { get; set; } = false;

    [Required]
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(AssetId))]
    public Asset? Asset { get; set; }
}
