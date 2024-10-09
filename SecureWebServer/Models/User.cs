using System.ComponentModel.DataAnnotations;

namespace SecureWebServer.Models
{
    public class User
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(80)]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(16)]
        public string UserName { get; set; }
        [Required]
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "general" or "admin"

        public bool IsApproved { get; set; }
        public ICollection<UserGroup>? UserGroups { get; set; }
    }
}





