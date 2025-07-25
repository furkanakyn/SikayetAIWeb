using Microsoft.EntityFrameworkCore;

namespace SikayetAIWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Response> Responses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Complaint)
                .WithMany(c => c.Responses)
                .HasForeignKey(r => r.ComplaintId);

            modelBuilder.Entity<Response>()
                .HasOne(r => r.Responder)
                .WithMany()
                .HasForeignKey(r => r.ResponderId);
        }
    }
}