using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models;

public class SwapSortRequest
{
    [Required]
    public int Id1 { get; set; }

    [Required]
    public int Id2 { get; set; }
}
