using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rovio.MatchMaking.Repositories.Data.Configurations
{
    class QueuedPlayerConfiguration : IEntityTypeConfiguration<QueuedPlayer>
    {
        public void Configure(EntityTypeBuilder<QueuedPlayer> entity)
        {
            entity.Property(e => e.Id)
                .HasColumnType("char(36)");
            entity.Property(e => e.PlayerId)
                .HasColumnType("char(36)");
        }
    }
}
