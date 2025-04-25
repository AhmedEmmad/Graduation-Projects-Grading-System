using GradingManagementSystem.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GradingManagementSystem.Repository.Data.ConfigurationClasses
{
    public class SupervisorGradingConfigurations : IEntityTypeConfiguration<SupervisorGrading>
    {
        public void Configure(EntityTypeBuilder<SupervisorGrading> builder)
        {
            // Composite Primary Key Of Foreign Keys
            builder.HasKey(SG => new { SG.StudentId, SG.GradingCriteriaId, SG.DoctorId });

            // One-To-Many (Students <-> SupervisorGradings)
            builder.HasOne(SG => SG.Student)
                   .WithMany(S => S.SupervisorGradings)
                   .HasForeignKey(SG => SG.StudentId)
                   .OnDelete(DeleteBehavior.NoAction);

            // One-To-Many (GradingCriterias <-> SupervisorGradings)
            builder.HasOne(SG => SG.GradingCriteria)
                   .WithMany(GC => GC.SupervisorGradings)
                   .HasForeignKey(SG => SG.GradingCriteriaId)
                   .OnDelete(DeleteBehavior.NoAction);

            // One-To-Many (Doctors <-> SupervisorGradings)
            builder.HasOne(SG => SG.Doctor)
                   .WithMany(D => D.SupervisorGradings)
                   .HasForeignKey(SG => SG.DoctorId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
