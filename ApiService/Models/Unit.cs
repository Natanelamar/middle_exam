using System.ComponentModel.DataAnnotations;

namespace ApiService.Models;

public class Unit
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string UnitName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sector { get; set; } = "General";

    public ICollection<Asset> Assets { get; set; } = new List<Asset>();
}
