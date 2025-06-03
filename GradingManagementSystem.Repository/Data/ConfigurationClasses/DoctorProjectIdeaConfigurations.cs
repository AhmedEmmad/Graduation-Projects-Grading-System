using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class DoctorProjectIdeaConfigurations : IEntityTypeConfiguration<DoctorProjectIdea>
    {
        public void Configure(EntityTypeBuilder<DoctorProjectIdea> builder)
        {
            builder.HasIndex(dp => dp.Name).IsUnique();
            
            // One-To-Many (Doctor <-> DoctorProjectIdea)
            builder.HasOne(dp => dp.Doctor)
                   .WithMany(d => d.DoctorProjectIdeas)
                   .HasForeignKey(dp => dp.DoctorId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
