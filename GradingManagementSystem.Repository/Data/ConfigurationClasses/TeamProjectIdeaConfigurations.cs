using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingSystem.Repository.Data.ConfigurationClasses
{
    public class TeamProjectIdeaConfigurations : IEntityTypeConfiguration<TeamProjectIdea>
    {
        public void Configure(EntityTypeBuilder<TeamProjectIdea> builder)
        {
            builder.HasIndex(tp => tp.Name)
                   .IsUnique();

            // One-To-Many (Team <-> TeamProjectIdea)
            builder.HasOne(tp => tp.Team)
                   .WithMany(t => t.TeamProjectIdeas)
                   .HasForeignKey(tp => tp.TeamId)
                   .OnDelete(DeleteBehavior.NoAction);

            // One-To-Many (Leader <-> TeamProjectIdea)
            builder.HasOne(tp => tp.Leader)
                   .WithMany(l => l.TeamProjectIdeas)
                   .HasForeignKey(tp => tp.LeaderId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
