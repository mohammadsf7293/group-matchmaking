using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Rovio.MatchMaking.Repositories.Data.Configurations
{
    class SessionPlayerConfiguration : IEntityTypeConfiguration<SessionPlayer>
    {
        public void Configure(EntityTypeBuilder<SessionPlayer> entity)
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
                  .HasDefaultValue(0); 
        }
    }
}
