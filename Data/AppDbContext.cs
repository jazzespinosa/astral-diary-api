using AstralDiaryApi.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AstralDiaryApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Entry> Entries { get; set; }
        public DbSet<Draft> Drafts { get; set; }
        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(User).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Entry).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Draft).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(Attachment).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}
