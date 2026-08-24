using System.ComponentModel.DataAnnotations;

namespace ApiService.DTOs;

public class CreateUnitDto
{


    [Required]
    [MaxLength(255)]
    public string UnitName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Sector { get; set; } = "General";

}
