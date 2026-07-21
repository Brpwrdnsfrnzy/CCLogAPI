namespace CCLogAPI.Models;

using System.ComponentModel.DataAnnotations;

public class CreateUserDto
{
    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(8)]
    public string Password { get; set; }

    public string Name { get; set; }

    public int Role { get; set; }

    public int Team { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public bool Assignable { get; set; }

    public bool EmailConfirmed { get; set; }
}
