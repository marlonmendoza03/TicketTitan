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
        public DbSet<StoryChatSession> StoryChatSessions => Set<StoryChatSession>();
        public DbSet<StoryChatMessage> StoryChatMessages => Set<StoryChatMessage>();
        public DbSet<StoryChatAttachment> StoryChatAttachments => Set<StoryChatAttachment>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoryChatSession>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

                entity.Property(x => x.UserId)
                .IsRequired();

                entity.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();
            });

            modelBuilder.Entity<StoryChatAttachment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

                entity.Property(x => x.FileName)
                .HasMaxLength(256)
                .IsRequired();

                entity.Property(x => x.StoragePath)
                .IsRequired();

                entity.Property(x => x.DownloadUrl)
                .IsRequired();

                entity.HasOne(x => x.Session)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<StoryChatMessage>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Role)
                .HasConversion<int>()
                .HasMaxLength(10)
                .IsRequired();

                entity.Property(x => x.Content)
                .IsRequired();

                entity.HasOne(x => x.Session)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.SessionId)
                .OnDelete(DeleteBehavior.Cascade);
            });

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
