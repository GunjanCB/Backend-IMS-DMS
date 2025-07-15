using Microsoft.EntityFrameworkCore;
using DocumentManagementBackend.Models;

namespace DocumentManagementBackend.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }
        public DbSet<UserOtp> UserOtps { get; set; }
        public DbSet<PendingUser> PendingUsers { get; set; }
        public DbSet<CapturedImage> CapturedImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CapturedImage>()
            .HasOne(ci => ci.User)
            .WithMany(u => u.CapturedImages)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}