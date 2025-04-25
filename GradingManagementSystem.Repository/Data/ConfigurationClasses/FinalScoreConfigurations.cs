using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class FinalScoreConfigurations : IEntityTypeConfiguration<FinalScore>
    {
        public void Configure(EntityTypeBuilder<FinalScore> builder)
        {
            builder.HasOne(fs => fs.Team)
                .WithOne(t => t.FinalScore)
                .HasForeignKey<FinalScore>(fs => fs.TeamId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
