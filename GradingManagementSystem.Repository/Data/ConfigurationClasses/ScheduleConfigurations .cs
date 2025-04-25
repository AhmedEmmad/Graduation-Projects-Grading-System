using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class ScheduleConfigurations : IEntityTypeConfiguration<Schedule>
    {
        public void Configure(EntityTypeBuilder<Schedule> builder)
        {
            builder.HasOne(s => s.Team)
                .WithMany(t => t.Schedules)
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
