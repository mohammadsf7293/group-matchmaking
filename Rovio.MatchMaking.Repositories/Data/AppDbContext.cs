using Microsoft.EntityFrameworkCore;
using Rovio.MatchMaking.Repositories.Data.Configurations;

namespace Rovio.MatchMaking.Repositories.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Session> Sessions { get; set; }
        public DbSet<QueuedPlayer> QueuedPlayers { get; set; }
        public DbSet<SessionPlayer> SessionPlayers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new SessionConfiguration());


            modelBuilder.Entity<SessionPlayer>(entity =>
            {
                entity.HasKey(e => e.Id); // Primary Key

                entity.Property(e => e.SessionId)
                      .IsRequired(); // Foreign key to Session

                entity.Property(e => e.PlayerId)
                      .IsRequired(); // Foreign key to Player

                entity.Property(e => e.Status)
                      .IsRequired()
                      .HasDefaultValue("ATTENDED"); // Default Status

                entity.Property(e => e.Score)
                      .IsRequired()
                      .HasDefaultValue(0); // Default Score
/*
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default creation timestamp

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP"); // Default updated timestamp
*/
            });

            modelBuilder.Entity<QueuedPlayer>()
                .Property(e => e.Id)
                .HasColumnType("char(36)");

            modelBuilder.Entity<QueuedPlayer>()
                .Property(e => e.PlayerId)
                .HasColumnType("char(36)");
        }
    }
}
