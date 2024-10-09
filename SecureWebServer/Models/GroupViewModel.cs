using System.ComponentModel.DataAnnotations;

namespace SecureWebServer.Models
{
    public class GroupViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }
    }

}
