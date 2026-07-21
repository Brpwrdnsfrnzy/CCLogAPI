using System.ComponentModel.DataAnnotations;

namespace CCLogAPI.Models
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        // Optional domain fields
        public int Role { get; set; }
        public int Team { get; set; }
    }
}