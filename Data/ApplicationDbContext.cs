using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketTitan.Models;

namespace TicketTitan.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Ticket> Tickets => Set<Ticket>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);

                entity.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);

                entity.Property(t => t.Status)
                .HasDefaultValue("Open");

                entity.Property(t => t.Priority)
                .HasDefaultValue(1);

                entity.HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.UpdatedByUser)
                .WithMany()
                .HasForeignKey(t => t.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            });
        }
    }
}
