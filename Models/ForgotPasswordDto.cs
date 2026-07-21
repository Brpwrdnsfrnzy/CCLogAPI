using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models;

public class ForgotPasswordDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
