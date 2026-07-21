namespace CCLogAPI.Models;

public class UserDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Name { get; set; }
    public int Role { get; set; }
    public int Team { get; set; }
    public string? PhoneNumber { get; set; }
    public bool Assignable { get; set; }
    public bool EmailConfirmed { get; set; }
}
