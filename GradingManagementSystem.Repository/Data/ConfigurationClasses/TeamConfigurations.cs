using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class TeamConfigurations : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.HasIndex(T => T.Name).IsUnique();

            // One-to-One (Teams <-> Leader (Students))
            builder.HasOne(T => T.Leader)
                   .WithOne(S => S.LeaderOfTeam)
                   .HasForeignKey<Team>(T => T.LeaderId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
