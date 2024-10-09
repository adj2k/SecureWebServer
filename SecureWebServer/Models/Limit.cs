namespace SecureWebServer.Models
{
    public class Limit
    {
        public int Id { get; set; }
        public int? UserId { get; set; } 
        public User User { get; set; }
        public int? GroupId { get; set; } 
        public Group Group { get; set; }
        public long MaxStorageBytes { get; set; }
        public long CurrentStorageBytes { get; set; } 
    }

}
