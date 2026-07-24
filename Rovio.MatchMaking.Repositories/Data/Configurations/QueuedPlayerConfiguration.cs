using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rovio.MatchMaking.Repositories.Data.Configurations;

class QueuedPlayerConfiguration : IEntityTypeConfiguration<QueuedPlayer>
{
    public void Configure(EntityTypeBuilder<QueuedPlayer> entity)
    {
        entity.ToTable(t =>
            t.HasCheckConstraint("CK_QueuedPlayers_LatencyLevel", "LatencyLevel >= 1 AND LatencyLevel <= 5"));

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasColumnType("char(36)");
        entity.Property(e => e.PlayerId)
            .HasColumnType("char(36)")
            .IsRequired();
        entity.Property(e => e.LatencyLevel)
            .IsRequired();
        entity.Property(e => e.CreatedAt)
            .IsRequired();

        entity.HasIndex(e => e.PlayerId)
            .IsUnique();
        entity.HasIndex(e => e.LatencyLevel);
        entity.HasIndex(e => e.CreatedAt);
    }
}
