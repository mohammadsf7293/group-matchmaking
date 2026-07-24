using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Rovio.MatchMaking;

namespace Rovio.MatchMaking.Repositories.Data.Configurations;

class SessionPlayerConfiguration : IEntityTypeConfiguration<SessionPlayer>
{
    public void Configure(EntityTypeBuilder<SessionPlayer> entity)
    {
        entity.ToTable(t =>
            t.HasCheckConstraint(
                "CK_SessionPlayers_Status",
                "Status IN ('ATTENDED', 'PLAYED', 'LEFT')"));

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id)
            .HasColumnType("char(36)");
        entity.Property(e => e.SessionId)
            .HasColumnType("char(36)")
            .IsRequired();
        entity.Property(e => e.PlayerId)
            .HasColumnType("char(36)")
            .IsRequired();
        entity.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue(SessionPlayerStatus.Attended);
        entity.Property(e => e.Score)
            .IsRequired()
            .HasDefaultValue(0);

        entity.HasOne<Session>()
            .WithMany()
            .HasForeignKey(e => e.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.SessionId);
        entity.HasIndex(e => e.PlayerId);
        entity.HasIndex(e => new { e.SessionId, e.PlayerId })
            .IsUnique();
    }
}
