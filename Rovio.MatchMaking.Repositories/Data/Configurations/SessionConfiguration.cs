using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rovio.MatchMaking.Repositories.Data.Configurations
{
    class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> entity)
        {
            entity.Property(e => e.Id).HasColumnType("char(36)");
            entity.Property(e => e.LatencyLevel)
                  .IsRequired()
                  .HasDefaultValue(1)
                  .HasAnnotation("SqlServer:Check", "LatencyLevel >= 1");

            entity.Property(e => e.JoinedCount)
                  .HasDefaultValue(0)
                  .HasAnnotation("SqlServer:Check", "JoinedCount <= 10");

            entity.Property(e => e.StartsAt)
                  .IsRequired();

            entity.Property(e => e.EndsAt)
                  .IsRequired();
        }
    }
}
