using Microsoft.EntityFrameworkCore;
using SecureWebServer.Models;

namespace SecureWebServer.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Limit> Limits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the UserGroup many-to-many relationship
            modelBuilder.Entity<UserGroup>()
                .HasKey(ug => new { ug.UserId, ug.GroupId });

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.User)
                .WithMany(u => u.UserGroups)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserGroup>()
                .HasOne(ug => ug.Group)
                .WithMany(g => g.UserGroups)
                .HasForeignKey(ug => ug.GroupId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Creator)
                .WithMany()
                .HasForeignKey(g => g.CreatorUserId)
                // Set the foreign key to null on deletion of the creator
                .OnDelete(DeleteBehavior.Restrict); 

            // Configure the foreign key between Item and UserCreator
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Creator)
                .WithMany()
                .HasForeignKey(i => i.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
