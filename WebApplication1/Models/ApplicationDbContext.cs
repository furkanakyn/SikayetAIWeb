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
        public DbSet<DepartmentCategory> DepartmentCategories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Enum tiplerinin veritabanına string olarak kaydedilmesini sağlar.
            modelBuilder.Entity<User>()
                .Property(u => u.UserType)
                .HasConversion<string>();

            modelBuilder.Entity<Complaint>()
                .Property(c => c.Status)
                .HasConversion<string>();

            // --- İLİŞKİ DÜZENLEMELERİ ---

            // Complaint - User ilişkisi: Bir şikayetin bir kullanıcısı vardır.
            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.User)
                .WithMany(u => u.Complaints)
                .HasForeignKey(c => c.UserId);

            // Response - Complaint ilişkisi: Bir yanıtın bir şikayete ait olduğu.
            modelBuilder.Entity<Response>()
                .HasOne(r => r.Complaint)
                .WithMany(c => c.Responses)
                .HasForeignKey(r => r.ComplaintId);

            // Response - User ilişkisi: Bir yanıtın hangi belediye veya admin kullanıcısından geldiği.
            modelBuilder.Entity<Response>()
                .HasOne(r => r.User)
                .WithMany(u => u.Responses)
                .HasForeignKey(r => r.UserId);

            // DepartmentCategory - User ilişkisi: Bir kullanıcının (Belediye) bir departmanı olduğu.
            // Bu, 'User' modelindeki 'DepartmentId' foreign key'ini kullanarak ilişkiyi kurar.
            modelBuilder.Entity<User>()
                .HasOne<DepartmentCategory>()
                .WithMany()
                .HasForeignKey(u => u.DepartmentId);

            // DepartmentCategory için birincil anahtar (Primary Key) tanımlaması.
            // Modelde DepartmentId'yi birincil anahtar olarak kullanıyor.
            modelBuilder.Entity<DepartmentCategory>()
                .HasKey(dc => dc.DepartmentId);

            base.OnModelCreating(modelBuilder);
        }
    }
}