using Homera.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Homera.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Location> Locations { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TaskItem>(entity =>
            {
                entity.Property(e => e.Budget)
                    .HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.Client)
                    .WithMany(u => u.CreatedTasks)
                    .HasForeignKey(t => t.ClientId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Housekeeper)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(t => t.HousekeeperId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
