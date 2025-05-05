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
            modelBuilder.ApplyConfiguration(new SessionPlayerConfiguration());

            modelBuilder.Entity<QueuedPlayer>()
                .Property(e => e.Id)
                .HasColumnType("char(36)");

            modelBuilder.Entity<QueuedPlayer>()
                .Property(e => e.PlayerId)
                .HasColumnType("char(36)");
        }
    }
}
