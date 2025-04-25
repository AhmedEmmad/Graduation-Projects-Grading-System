using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class CommitteeAssignmentConfigurations : IEntityTypeConfiguration<CommitteeAssignment>
    {
        public void Configure(EntityTypeBuilder<CommitteeAssignment> builder)
        {
            builder.HasOne(ca => ca.Committee)
                .WithMany(c => c.CommitteeAssignments)
                .HasForeignKey(ca => ca.CommitteeId);

            builder.HasOne(ca => ca.Team)
                .WithMany(t => t.CommitteeAssignments)
                .HasForeignKey(ca => ca.TeamId);
        }
    }
}
