using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models;

public class CreateDiaryEntryDto
{
    [Required]
    public int CreatedBy { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public string Text { get; set; } = string.Empty;

    [Required]
    public int LogId { get; set; }

    public int? ParentId { get; set; }
}
