using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models;

public class AdminResetPasswordDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    [MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    public bool RequirePasswordChange { get; set; } = true;
}
