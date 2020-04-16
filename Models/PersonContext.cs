using Microsoft.EntityFrameworkCore;

namespace REST_MySQL.Models
{
    public class PersonContext : DbContext
    {
        public PersonContext(DbContextOptions<PersonContext> options) : base(options)
        {
            this.Database.EnsureCreated();
        }

        public DbSet<Person> People { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(e => e.Uid);
                // entity.Property(e => e.Title).IsRequired();
            });
        }
    }
}
