using System.ComponentModel.DataAnnotations;

namespace SecureWebServer.Models
{
    public class Item
    {
        public int Id { get; set; }

        [Required]
        [StringLength(68, ErrorMessage ="Item Name cannot be longer than 68 characters.")]
        public string ItemName { get; set; }
        public long? FileSize { get; set; }
        public string? FileType { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        [StringLength(500, ErrorMessage = "Description cannot be longer than 68 characters.")]
        public string ItemDescription { get; set; }
        public int CreatorId { get; set; }
        public User Creator { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessedAt { get; set; }

        
    }

}
