using Core.Entities.Auth;
using Core.Entities.Chat;
using Microsoft.EntityFrameworkCore;

namespace Repository.Context
{
    public class CarFinderDbContext : DbContext
    {
        public CarFinderDbContext(DbContextOptions<CarFinderDbContext> options) : base(options)
        {
        }

        // Auth DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<EmailOtpToken> EmailOtpTokens { get; set; }

        // Chat DbSets
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.ProfileImageUrl).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                
                // Relationships
                entity.HasMany(e => e.RefreshTokens)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
                entity.Property(e => e.CreatedByIp).HasMaxLength(45);
                entity.Property(e => e.RevokedByIp).HasMaxLength(45);
                entity.Property(e => e.ReasonRevoked).HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // ChatSession configuration
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.UserId).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.IsCompleted).HasDefaultValue(false);

                // Relationships
                entity.HasMany(e => e.Messages)
                    .WithOne(e => e.Session)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ChatMessage configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SessionId).IsRequired();
                entity.Property(e => e.Sender).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Message).IsRequired().HasColumnType("NVARCHAR(MAX)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Index for performance
                entity.HasIndex(e => new { e.SessionId, e.CreatedAt });
            });
        }
    }
}