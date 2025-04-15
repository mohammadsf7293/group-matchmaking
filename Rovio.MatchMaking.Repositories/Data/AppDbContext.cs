using Microsoft.EntityFrameworkCore;

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

            modelBuilder.Entity<Session>(entity =>
            {
                entity.Property(e => e.Id).HasColumnType("char(36)");
                entity.Property(e => e.LatencyLevel)
                      .IsRequired()
                      .HasDefaultValue(1)
                      .HasAnnotation("SqlServer:Check", "LatencyLevel >= 1");

                entity.Property(e => e.JoinedCount)
                      .HasDefaultValue(0)
                      .HasAnnotation("SqlServer:Check", "JoinedCount <= 10");
/*
                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");
*/
                entity.Property(e => e.StartsAt)
                      .IsRequired();

                entity.Property(e => e.EndsAt)
                      .IsRequired();
            });


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
