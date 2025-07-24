using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace SikayetAIWeb.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Complaint> Complaints { get; set; }
    }

}
