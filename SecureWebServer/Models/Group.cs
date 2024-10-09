using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SecureWebServer.Models
{
    public class Group
    {
        public int GroupId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        [ForeignKey("CreatorUserId")]
        public int CreatorUserId { get; set; }
        
        public User Creator { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsApproved { get; set; }

        public ICollection<UserGroup> UserGroups { get; set; }

        public ICollection<Item> Items { get; set; }
    }

}
