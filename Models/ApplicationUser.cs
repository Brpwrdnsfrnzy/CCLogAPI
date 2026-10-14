using Microsoft.AspNetCore.Identity;

namespace CCLogAPI.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
    public int Role { get; set; }
    public int Team { get; set; }
}
