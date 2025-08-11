using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SikayetAIWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<Response> Responses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<Message> Messages { get; set; }

        // Yeni eklediğiniz category_departments tablosu için DbSet
        public DbSet<CategoryDepartmentMapping> CategoryDepartmentMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enum tiplerinin veritabanına string olarak kaydedilmesini sağlar.
            modelBuilder.Entity<User>()
                .Property(u => u.UserType)
                .HasConversion<string>();

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // Complaint - User ilişkisi: Bir şikayetin bir kullanıcısı vardır.
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId);

            // Response - Complaint ilişkisi
            modelBuilder.Entity<Response>()
                .HasOne(r => r.Complaint)
                .WithMany(c => c.Responses)
                .HasForeignKey(r => r.ComplaintId);

            // Response - User ilişkisi
            modelBuilder.Entity<Response>()
                .HasOne(r => r.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(r => r.UserId);

            // User - Department ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull); // isteğe bağlı, silinince null olsun

            // Department için primary key tanımı
            modelBuilder.Entity<Department>()
                .HasKey(d => d.DepartmentId);

            base.OnModelCreating(modelBuilder);
        }
    }
}
