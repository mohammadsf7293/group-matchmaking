using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rovio.MatchMaking.Repositories.Data.Configurations;

class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> entity)
    {
        entity.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Sessions_LatencyLevel", "LatencyLevel >= 1 AND LatencyLevel <= 5");
            t.HasCheckConstraint("CK_Sessions_JoinedCount", "JoinedCount >= 0 AND JoinedCount <= 10");
        });

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasColumnType("char(36)");
        entity.Property(e => e.LatencyLevel)
            .IsRequired()
            .HasDefaultValue(1);
        entity.Property(e => e.JoinedCount)
            .HasDefaultValue(0)
            .IsConcurrencyToken();
        entity.Property(e => e.StartsAt)
            .IsRequired();
        entity.Property(e => e.EndsAt)
            .IsRequired();

        entity.HasIndex(e => e.LatencyLevel);
        entity.HasIndex(e => e.EndsAt);
        entity.HasIndex(e => new { e.LatencyLevel, e.EndsAt, e.JoinedCount });
    }
}
